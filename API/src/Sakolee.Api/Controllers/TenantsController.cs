using Sakolee.Api.Models;
using Sakolee.Api.Models.Profile;
using Sakolee.Api.Models.Tenants;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Email;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Common;
using Sakolee.Application.OptionSets;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Tenant management (WO-40): tenant lifecycle — create, update, status, and archive (Super Admin)
/// — plus tenant detail reads.
/// </summary>
[ApiController]
[Route("/api/admin/tenants")]
[Produces("application/json")]
[Tags("Tenants")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class TenantsController : ControllerBase
{
    #region Fields & Constructor

    private readonly ITenantRepository _tenants;
    private readonly IUserRepository _users;
    private readonly IPersonRepository _persons;
    private readonly IAddressRepository _addresses;
    private readonly IRoleRepository _roles;
    private readonly IOptionSetRepository _optionSets;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICredentialEncryptionService _credentialEncryption;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailNotificationService _emailNotifications;
    private readonly IEmailDispatcher _emailDispatcher;

    public TenantsController(
        ITenantRepository tenants,
        IUserRepository users,
        IPersonRepository persons,
        IAddressRepository addresses,
        IRoleRepository roles,
        IOptionSetRepository optionSets,
        IPasswordHasher passwordHasher,
        ICredentialEncryptionService credentialEncryption,
        IRefreshTokenRepository refreshTokens,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork,
        IEmailNotificationService emailNotifications,
        IEmailDispatcher emailDispatcher)
    {
        _tenants = tenants;
        _users = users;
        _persons = persons;
        _addresses = addresses;
        _roles = roles;
        _optionSets = optionSets;
        _passwordHasher = passwordHasher;
        _credentialEncryption = credentialEncryption;
        _refreshTokens = refreshTokens;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _emailNotifications = emailNotifications;
        _emailDispatcher = emailDispatcher;
    }

    #endregion

    #region Tenant Lifecycle (Super Admin)

    /// <summary>
    /// Creates a new tenant with a unique <see cref="CreateTenantRequest.Identifier"/>, seeds it with its
    /// own copy of the platform's default option lists (so its admins can manage values independently of
    /// the shared originals), and starts it in <see cref="TenantStatus.Active"/>.
    /// <para>
    /// Also mints the tenant's default Administrator: an <see cref="Address"/> (when supplied, stored in
    /// the Addresses table and referenced by <see cref="Tenant.AddressId"/>), a <see cref="Person"/> (the
    /// User Information fields, stored in the Persons table and referenced by
    /// <see cref="Tenant.PersonId"/>), and a <see cref="User"/> login holding the "Administrator" role in
    /// this tenant. That login is <see cref="User.IsProtected"/> — it can never be deactivated or have its
    /// tenant role assignment removed, guaranteeing the tenant always keeps a working admin.
    /// </para>
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.TenantsWrite)]
    [ProducesResponseType<ApiResponse<CreateTenantResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        if (await _tenants.IdentifierExistsAsync(request.Identifier, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Identifier already in use.", request.Identifier));
        }

        var email = request.Email.Trim();
        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateEmail, "Email already in use.", email));
        }

        // The platform-scoped "Administrator" role — required for the default admin's assignment. Not a
        // seeded system role, so it must already exist (created once via the Roles screen); this does not
        // create or modify it.
        var adminRole = await _roles.GetByNameAsync("Administrator", cancellationToken)
            ?? throw new InvalidOperationException("The Administrator role was not found.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Identifier = request.Identifier,
            TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "UTC" : request.TimeZoneId,
            Status = TenantStatus.Active,
            CreatedDate = DateTime.UtcNow,
        };

        var adminUserId = Guid.NewGuid();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var temporaryPassword = _passwordHasher.GenerateTemporaryPassword();

        // Two SaveChanges calls (see below) have to commit as one unit — a failure partway through must
        // not leave a tenant with no admin login, or an admin Person with no tenant.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _tenants.AddAsync(tenant, ct);

            // The new tenant gets its OWN copy of the platform's default option lists, so its admins can
            // manage the values (add / rename / delete / re-order) without touching the shared originals.
            await TenantOptionSetSeeder.EnsureDefaultsAsync(_optionSets, tenant.Id, ct);

            // The tenant's own address (optional) — Addresses table, referenced by Tenant.AddressId.
            if (request.Address is { } addressInput)
            {
                await UpsertTenantAddressAsync(tenant, addressInput, ct);
            }

            // The default Administrator's CRM master record — User Information fields, Persons table,
            // referenced by Tenant.PersonId. Mirrors how Student/User creation each mint their own Person.
            var person = new Person
            {
                Id = Guid.NewGuid(),
                PersonCode = await GeneratePersonCodeAsync(ct),
                UserId = adminUserId,
                FirstName = firstName,
                LastName = lastName,
                DisplayName = fullName,
                PrimaryEmail = email,
                MobileNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber,
                CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? null : request.CountryCode,
                TenantId = tenant.Id,
                IsActive = true,
                LastProfileUpdatedOn = DateTime.UtcNow,
                // Created via the Tenant screen, not entered on the Person screen itself.
                SourceEntityType = EntityType.Tenant,
            };
            person.TenantMappings.Add(new TenantPersonMapping { Id = Guid.NewGuid(), TenantId = tenant.Id });
            await _persons.AddAsync(person, ct);

            // Tenant.PersonId and Person.TenantId each reference a row the other side is inserting in
            // this same request — a genuine cycle EF cannot resolve in one batch (it would need to insert
            // both rows before either FK could be non-null). Saved here, with Tenant.PersonId still null,
            // so only Person -> Tenant is live for this insert; the link back is set as an UPDATE below.
            await _unitOfWork.SaveChangesAsync(ct);
            tenant.PersonId = person.Id;

            // The Administrator's login — protected (see User.IsProtected) so the tenant can never be
            // left without a working admin.
            var (hash, salt) = _passwordHasher.Hash(temporaryPassword);
            var adminUser = new User
            {
                Id = adminUserId,
                Email = email,
                DisplayName = fullName,
                PersonId = person.Id,
                PasswordHash = hash,
                Salt = salt,
                IsActive = true,
                IsProtected = true,
                MustChangePassword = true,
                TokenVersion = 1,
                CreatedDate = DateTime.UtcNow,
                // Kept (encrypted) only until the admin signs in and sets their own password, so
                // "Send Credentials" can resend this exact one without minting a new one.
                EncryptedTemporaryPassword = _credentialEncryption.Encrypt(temporaryPassword),
            };
            await _users.AddAsync(adminUser, ct);
            await _users.AddAssignmentAsync(new UserTenantRole
            {
                Id = Guid.NewGuid(),
                UserId = adminUserId,
                TenantId = tenant.Id,
                Role = RoleAssignment.MapLegacyRole(adminRole, null),
                RoleId = adminRole.Id,
            }, ct);

            await _audit.AddAsync(nameof(Tenant), tenant.Id.ToString(), "Created",
                details: $"{tenant.Identifier}; admin={email}", cancellationToken: ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(
                new CreateTenantResponse(tenant.Id, tenant.Identifier, tenant.Status.ToString(), adminUserId, temporaryPassword),
                "Tenant created."));
    }

    /// <summary>What the Tenants list may be ordered by.</summary>
    private static readonly SortMap<Tenant> Sorts = new SortMap<Tenant>("updatedOnUtc")
        .Add("name", t => t.Name)
        .Add("identifier", t => t.Identifier)
        .Add("status", t => t.Status, t => t.UpdatedOnUtc)
        .Add("timeZoneId", t => t.TimeZoneId)
        .Add("createdOnUtc", t => t.CreatedOnUtc)
        .Add("updatedOnUtc", t => t.UpdatedOnUtc);

    /// <summary>
    /// Paginated list of tenants (Super Admin only), with optional status/search filters. Archived
    /// tenants are excluded unless <paramref name="includeArchived"/> is set — the whole set is read and
    /// filtered/sorted in memory (the tenant table is small), so paging happens after ordering.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.TenantsWrite)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool includeArchived = false,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var all = await _tenants.ListAsync(cancellationToken);
        IEnumerable<Tenant> filteredSet = includeArchived ? all : all.Where(t => t.Status != TenantStatus.Archived);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, ignoreCase: true, out var statusFilter))
        {
            filteredSet = filteredSet.Where(t => t.Status == statusFilter);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filteredSet = filteredSet.Where(t =>
                t.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.Identifier.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // Ordered before it is paged. This list is small enough to be read whole and filtered in memory,
        // but "page 1" still means the first rows OF AN ORDER, so the order has to be settled first.
        var filtered = Sorts.Apply(filteredSet, sortBy, descending).ToList();
        var pageTenants = filtered.Skip((page - 1) * limit).Take(limit).ToList();
        var names = await ResolveActorNamesAsync(pageTenants.SelectMany(t => new[] { t.CreatedById, t.UpdatedById }), cancellationToken);
        var pageItems = pageTenants.Select(t => new TenantSummary(
            t.Id, t.Name, t.Identifier, t.Status.ToString(), t.TimeZoneId,
            NameOf(names, t.CreatedById), NameOf(names, t.UpdatedById), t.CreatedOnUtc, t.UpdatedOnUtc));

        return Ok(ApiResponseFactory.Paginated(pageItems, "Tenants retrieved.", page, limit, filtered.Count));
    }

    /// <summary>Gets a single tenant's detail, including its provenance (who created/last updated it).</summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.TenantsWrite)]
    [ProducesResponseType<ApiResponse<TenantDetail>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return NotFound(ApiResponseFactory.Error(ApiErrorCodes.TenantNotFound, "Tenant not found.", id.ToString()));
        }

        var detail = new TenantDetail(
            tenant.Id, tenant.Name, tenant.Identifier, tenant.Status.ToString(), tenant.TimeZoneId,
            tenant.Address is null ? null : PersonProfileMapper.MapAddress(tenant.Address),
            await RecordAudit.ForAsync(_users, tenant, cancellationToken));

        return Ok(ApiResponseFactory.Success(detail, "Tenant retrieved."));
    }

    /// <summary>
    /// Updates a tenant's name and time zone. The <see cref="Tenant.Identifier"/> is immutable — it is
    /// never accepted from this request — since it is baked into the tenant's subdomain/routing.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.TenantsWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return NotFound(ApiResponseFactory.Error(ApiErrorCodes.TenantNotFound, "Tenant not found.", id.ToString()));
        }

        tenant.Name = request.Name; // identifier is immutable
        if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
        {
            tenant.TimeZoneId = request.TimeZoneId;
        }
        if (request.Address is { } addressInput)
        {
            await UpsertTenantAddressAsync(tenant, addressInput, cancellationToken);
        }
        _tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(
            new TenantResponse(tenant.Id, tenant.Identifier, tenant.Status.ToString()), "Tenant updated."));
    }

    /// <summary>
    /// Sends the tenant's default Administrator their login credentials by email — a one-click "get them
    /// working credentials, whatever it takes" action. When a temporary password is still known (kept
    /// encrypted at rest — see <see cref="User.EncryptedTemporaryPassword"/>) it re-sends that exact one,
    /// unchanged. When none is known — an account created before this field existed, or one whose admin
    /// already signed in and set their own password — it mints a fresh one instead (same rules as
    /// <c>UsersController.ResetPassword</c>: forces a change on next login, invalidates existing sessions).
    /// Either way the email goes out via the tenant's active SMTP account using the "User Invitation"
    /// template (a welcome message carrying the login email and temporary password).
    /// </summary>
    [HttpPost("{id:guid}/send-credentials")]
    [RequirePermission(Permissions.TenantsWrite)]
    [ProducesResponseType<ApiResponse<SendTenantCredentialsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendCredentials(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return NotFound(ApiResponseFactory.Error(ApiErrorCodes.TenantNotFound, "Tenant not found.", id.ToString()));
        }

        // Cross-tenant (Super Admin) read: the ambient tenant filter would otherwise hide this Person
        // whenever the caller's active tenant isn't the one being managed — i.e. almost always here.
        var person = tenant.PersonId is { } personId ? await _persons.GetByIdUnscopedAsync(personId, cancellationToken) : null;
        var admin = person?.UserId is { } adminUserId ? await _users.GetByIdAsync(adminUserId, cancellationToken) : null;
        if (admin is null)
        {
            return NotFound(ApiResponseFactory.NotFound("This tenant has no administrator account."));
        }

        string temporaryPassword;
        var passwordWasReset = string.IsNullOrEmpty(admin.EncryptedTemporaryPassword);
        if (passwordWasReset)
        {
            // Nothing saved to resend — mint a fresh one, same as Reset Password.
            temporaryPassword = _passwordHasher.GenerateTemporaryPassword();
            var (hash, salt) = _passwordHasher.Hash(temporaryPassword);
            admin.PasswordHash = hash;
            admin.Salt = salt;
            admin.MustChangePassword = true;
            admin.TokenVersion++; // invalidate all existing sessions
            admin.EncryptedTemporaryPassword = _credentialEncryption.Encrypt(temporaryPassword);
            _users.Update(admin);
            await _refreshTokens.RevokeAllForUserAsync(admin.Id, cancellationToken);
        }
        else
        {
            temporaryPassword = _credentialEncryption.Decrypt(admin.EncryptedTemporaryPassword!);
        }

        await _audit.AddAsync(nameof(User), admin.Id.ToString(), passwordWasReset ? "CredentialsSent (new password)" : "CredentialsResent",
            details: $"tenant={tenant.Identifier}", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Email the temporary password in the background via the tenant's active SMTP account; the flag
        // reflects whether a send will be attempted (so the caller can share manually otherwise).
        var emailSent = await _emailNotifications.HasActiveSenderAsync(tenant.Id, cancellationToken);
        _emailDispatcher.Enqueue(tenant.Id, EmailTemplateKey.UserInvitation, admin.Email,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["FullName"] = admin.DisplayName,
                ["Email"] = admin.Email,
                ["TemporaryPassword"] = temporaryPassword,
            });

        return Ok(ApiResponseFactory.Success(
            new SendTenantCredentialsResponse(tenant.Id, admin.Id, temporaryPassword, emailSent, passwordWasReset), "Credentials sent."));
    }

    /// <summary>
    /// Activates or deactivates a tenant (toggles between <see cref="TenantStatus.Active"/> and
    /// <see cref="TenantStatus.Inactive"/>). Distinct from <see cref="Archive"/>: this is reversible and
    /// does not require the elevated <see cref="Permissions.TenantsArchive"/> permission.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [RequirePermission(Permissions.TenantsWrite)]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] UpdateTenantStatusRequest request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return NotFound(ApiResponseFactory.Error(ApiErrorCodes.TenantNotFound, "Tenant not found.", id.ToString()));
        }

        tenant.Status = request.IsActive ? TenantStatus.Active : TenantStatus.Inactive;
        _tenants.Update(tenant);
        await _audit.AddAsync(nameof(Tenant), tenant.Id.ToString(), request.IsActive ? "Activated" : "Deactivated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { tenantId = tenant.Id, status = tenant.Status.ToString() }, "Status updated."));
    }

    /// <summary>
    /// Archives a tenant (<see cref="TenantStatus.Archived"/>) — the terminal, effectively-retired state
    /// hidden from <see cref="List"/> by default. Gated behind the separate
    /// <see cref="Permissions.TenantsArchive"/> permission since it is a heavier action than a status
    /// toggle.
    /// </summary>
    [HttpPut("{id:guid}/archive")]
    [RequirePermission(Permissions.TenantsArchive)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(id, cancellationToken);
        if (tenant is null)
        {
            return NotFound(ApiResponseFactory.Error(ApiErrorCodes.TenantNotFound, "Tenant not found.", id.ToString()));
        }

        tenant.Status = TenantStatus.Archived;
        _tenants.Update(tenant);
        await _audit.AddAsync(nameof(Tenant), tenant.Id.ToString(), "Archived", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { tenantId = tenant.Id, status = tenant.Status.ToString() }, "Tenant archived."));
    }

    #endregion

    #region Helpers

    /// <summary>Resolves the display names of the given user ids (nulls skipped), for the CreatedBy/UpdatedBy columns.</summary>
    private async Task<IReadOnlyDictionary<Guid, string>> ResolveActorNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        => await _users.GetFullNamesAsync(ids.Where(id => id.HasValue).Select(id => id!.Value), cancellationToken);

    /// <summary>Looks up a resolved actor name by id, or null when the id is absent or unresolved.</summary>
    private static string? NameOf(IReadOnlyDictionary<Guid, string> names, Guid? id)
        => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : null;

    /// <summary>Upserts the tenant's own address (Addresses table, referenced by <see cref="Tenant.AddressId"/>).</summary>
    private async Task UpsertTenantAddressAsync(Tenant tenant, AddressInput input, CancellationToken cancellationToken)
    {
        var address = tenant.AddressId is { } addressId
            ? await _addresses.GetByIdAsync(addressId, cancellationToken)
            : null;

        var isNew = address is null;
        address ??= new Address { Id = Guid.NewGuid() };

        address.AddressType = Enum.TryParse<AddressType>(input.AddressType, ignoreCase: true, out var type) ? type : AddressType.Home;
        address.AddressLine1 = input.AddressLine1;
        address.AddressLine2 = input.AddressLine2;
        address.Landmark = input.Landmark;
        address.BuildingName = input.BuildingName;
        address.FloorNumber = input.FloorNumber;
        address.UnitNumber = input.UnitNumber;
        address.CountryCode = input.CountryCode;
        address.CountryName = input.CountryName;
        address.StateCode = input.StateCode;
        address.StateName = input.StateName;
        address.CityName = input.CityName;
        address.PostalCode = input.PostalCode;

        if (isNew)
        {
            await _addresses.AddAsync(address, cancellationToken);
            tenant.AddressId = address.Id;
        }
        else
        {
            _addresses.Update(address);
        }
    }

    /// <summary>Generates a unique business person code (e.g. PER-AB12CD34EF) for the default Administrator.</summary>
    private async Task<string> GeneratePersonCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = "PER-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            if (!await _persons.PersonCodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        return "PER-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
    }

    #endregion
}
