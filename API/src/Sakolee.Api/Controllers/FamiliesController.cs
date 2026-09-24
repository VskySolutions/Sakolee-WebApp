using Sakolee.Api.Models.Families;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Abstractions.Tenancy;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Family management: create, list, update, and delete family (household) records — the <c>Families</c>
/// table, renamed from the legacy <c>Parents</c> table (see <see cref="Family"/> remarks).
/// <para>
/// A family is never created standalone — every family carries a primary contact inlined on the row
/// itself (see <see cref="Family"/> remarks), and creating one mints the CRM <see cref="Person"/> master
/// record for that contact (and, when supplied, a secondary contact) plus a login account for each — the
/// same way <c>StudentsController.Create</c> mints a Person and login for a new student. Every contact,
/// primary and secondary alike, also gets its own <see cref="FamilyPersonMapping"/> row under the
/// family's id — see <see cref="Create"/>.
/// </para>
/// <para>
/// Students enrol UNDER a family via <see cref="Student.FamilyId"/> (the FK onto <see cref="Family.Id"/>
/// — <c>FK_Students_Families</c>, renamed from <c>FK_Students_Parents</c>, which already existed on the
/// live schema before this controller did, its column renamed from <c>ParentId</c> by
/// <c>RenameStudentsParentIdToFamilyId</c>); this controller surfaces them read-only on
/// <see cref="GetById"/> but students are created/edited
/// through <c>StudentsController</c>.
/// </para>
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/families")]
[Produces("application/json")]
[Tags("Families")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
public sealed class FamiliesController : ControllerBase
{
    #region Field Declarations

    private readonly IFamilyRepository _families;
    private readonly IPersonRepository _persons;
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IStudentRepository _students;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;
    private readonly IActorAccessor _actorAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _audit;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FamiliesController"/> class.
    /// </summary>
    public FamiliesController(
        IFamilyRepository families,
        IPersonRepository persons,
        IUserRepository users,
        IRoleRepository roles,
        IStudentRepository students,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext,
        IActorAccessor actorAccessor,
        IUnitOfWork unitOfWork,
        IAuditTrailService audit)
    {
        _families = families;
        _persons = persons;
        _users = users;
        _roles = roles;
        _students = students;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
        _actorAccessor = actorAccessor;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    #endregion

    #region API Endpoints

    #region Create Endpoint

    /// <summary>
    /// Creates a new family. Mints the primary contact's CRM Person + login account (Parent role) and
    /// its own <see cref="FamilyPersonMapping"/> row, the Family (<c>Families</c>) row itself with the
    /// primary contact's identity inlined, and — when supplied — the same for a secondary contact.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.FamiliesWrite)]
    [ProducesResponseType<ApiResponse<FamilyResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        // Creating a family's Person/login records needs a tenant to own them.
        if (!_tenantContext.IsResolved)
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "An active tenant is required to create a family."));
        }
        var tenantId = _tenantContext.TenantId;

        // Reject a duplicate family name within the tenant before minting anything.
        if (await _families.FamilyNameExistsAsync(tenantId, request.FamilyName.Trim(), cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "A family with this name already exists.", request.FamilyName));
        }

        // Validate both contacts' emails up front — every contact gets a login account, so its email
        // must be free, and the two contacts on one family cannot share an email.
        var primaryEmail = request.Email.Trim();
        if (await _users.EmailExistsAsync(primaryEmail, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", primaryEmail));
        }
        if (request.SecondaryContact is { } secondary)
        {
            var secondaryEmail = secondary.Email.Trim();
            if (string.Equals(secondaryEmail, primaryEmail, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponseFactory.Error(
                    ApiErrorCodes.ValidationFailed, "Validation failed.", "The two contacts cannot share the same email."));
            }
            if (await _users.EmailExistsAsync(secondaryEmail, cancellationToken))
            {
                return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", secondaryEmail));
            }
        }

        // Created at startup when missing (BootstrapSeeder) — see Roles.Parent remarks.
        var parentRole = await _roles.GetByNameAsync(Roles.Parent, cancellationToken)
            ?? throw new InvalidOperationException("The Parent role was not found.");

        var now = DateTime.UtcNow;
        var actorId = CurrentActorId();

        // 1. The primary contact's CRM Person + login account.
        var (primaryPerson, primaryTempPassword) = await MintContactAsync(
            request.FirstName, request.LastName, primaryEmail, request.CellPhone, tenantId, parentRole, now, cancellationToken);

        // 2. The Family (Parents) row itself, with the primary contact's identity inlined.
        var family = new Family
        {
            Id = Guid.NewGuid(),
            PersonId = primaryPerson.Id,
            TenantId = tenantId,
            StudioLocationId = request.StudioLocationId,
            FamilyStatusId = request.FamilyStatusId,
            FamilyName = request.FamilyName.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            Type = request.Relation?.Trim(),
            Email = primaryEmail,
            HomePhone = request.HomePhone?.Trim(),
            WorkPhone = request.WorkPhone?.Trim(),
            CellPhone = request.CellPhone?.Trim(),
            Fax = request.Fax?.Trim(),
            OtherPhone = request.OtherPhone?.Trim(),
            Address1 = request.Address1?.Trim(),
            Address2 = request.Address2?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            ZipCode = request.ZipCode,
            IsPrimaryContact = true,
            IsBillingContact = request.IsBillingContact,
            IsAuthorizedToPickUpStudent = request.IsAuthorizedToPickUpStudent,
            Source = request.Source?.Trim(),
            ReferralName = request.ReferralName?.Trim(),
            EmergencyContactPerson = request.EmergencyContactPerson?.Trim(),
            EmergencyPhone = request.EmergencyPhone?.Trim(),
            HealthInsuranceCarrier = request.HealthInsuranceCarrier?.Trim(),
            Active = true,
            CreatedOnUtc = now,
            CreatedById = actorId,
            UpdatedOnUtc = now,
            UpdatedById = actorId,
        };
        await _families.AddAsync(family, cancellationToken);

        // 3. The primary contact's own FamilyPersonMapping row — Family's own inlined fields above stay
        // in sync as a legacy-shaped denormalized copy (see Family remarks), but FamilyPersonMapping is
        // the authoritative list of a family's contacts: every contact, primary included, is one row
        // here under the same FamilyId.
        var primaryMapping = BuildContactMappingRow(
            family.Id, primaryPerson.Id, request.FirstName, request.LastName, primaryEmail, request.CellPhone,
            request.Relation, isPrimary: true, request.IsBillingContact, request.IsAuthorizedToPickUpStudent,
            now, actorId);
        await _families.AddContactAsync(primaryMapping, cancellationToken);

        var contactResponses = new List<FamilyContactResponse>
        {
            new(primaryMapping.Id, primaryPerson.Id, request.FirstName.Trim(), request.LastName.Trim(), primaryEmail,
                request.CellPhone?.Trim(), primaryMapping.Relation, true, primaryMapping.IsBillingContact,
                primaryMapping.IsAuthorizedToPickUpStudent, primaryTempPassword)
        };

        // 4. The (optional) secondary contact — its own Person + login + FamilyPersonMapping row.
        if (request.SecondaryContact is { } secondaryInput)
        {
            var (secondaryPerson, secondaryTempPassword) = await MintContactAsync(
                secondaryInput.FirstName, secondaryInput.LastName, secondaryInput.Email.Trim(), secondaryInput.Phone,
                tenantId, parentRole, now, cancellationToken);

            var secondaryMapping = BuildContactMappingRow(
                family.Id, secondaryPerson.Id, secondaryInput.FirstName, secondaryInput.LastName, secondaryInput.Email.Trim(),
                secondaryInput.Phone, secondaryInput.Relation, isPrimary: false, secondaryInput.IsBillingContact,
                secondaryInput.IsAuthorizedToPickUpStudent, now, actorId);
            await _families.AddContactAsync(secondaryMapping, cancellationToken);

            contactResponses.Add(new FamilyContactResponse(
                secondaryMapping.Id, secondaryPerson.Id, secondaryInput.FirstName.Trim(), secondaryInput.LastName.Trim(),
                secondaryMapping.EmailAddress, secondaryMapping.PhoneNumber, secondaryMapping.Relation, false,
                secondaryMapping.IsBillingContact, secondaryMapping.IsAuthorizedToPickUpStudent, secondaryTempPassword));
        }

        // Persist everything staged above and record the audit trail entry in the same transaction.
        await _audit.AddAsync(nameof(Family), family.Id.ToString(), "Created", details: family.FamilyName, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(new FamilyResponse(family.Id, family.FamilyName, contactResponses), "Family created."));
    }

    #endregion

    #region List Endpoint

    /// <summary>
    /// Retrieves a paginated list of family records, scoped to the caller's active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.FamiliesRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? familyStatusId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination boundary values.
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        // Fetch the filtered, sorted, and paginated set from the repository.
        var tenantId = _tenantContext.IsResolved ? _tenantContext.TenantId : (Guid?)null;
        var (items, total) = await _families.ListAsync(
            search, tenantId, familyStatusId, new SortRequest(sortBy, descending), page, limit, cancellationToken);

        // Batch-resolve actor display names and per-family student counts, then project to summaries.
        var names = await ResolveActorNamesAsync(items.SelectMany(f => new[] { f.CreatedById, f.UpdatedById }), cancellationToken);
        var studentCounts = await _students.CountByFamilyIdsAsync(items.Select(f => f.Id), cancellationToken);
        var summaries = items.Select(f => ToSummary(f, names, studentCounts));

        return Ok(ApiResponseFactory.Paginated(summaries, "Families retrieved.", page, limit, total));
    }

    #endregion

    #region GetById Endpoint

    /// <summary>
    /// Retrieves a single family's full detail — its own fields, every contact, and every enrolled
    /// student — handling tenant scoping via <see cref="LoadAsync"/>.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.FamiliesRead)]
    [ProducesResponseType<ApiResponse<FamilyDetail>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var family = await LoadAsync(id, cancellationToken);
        if (family is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family not found."));
        }

        // Batch-load the family's enrolled students and their linked Persons (for display names), plus
        // the actor names behind the family's own audit trail.
        var students = await _students.ListByFamilyIdAsync(family.Id, cancellationToken);
        var studentPersonIds = students.Where(s => s.PersonId.HasValue).Select(s => s.PersonId!.Value).Distinct().ToList();
        var studentPersons = studentPersonIds.Count > 0
            ? (await _persons.GetByIdsAsync(studentPersonIds, cancellationToken)).ToDictionary(p => p.Id)
            : new Dictionary<Guid, Person>();
        var names = await ResolveActorNamesAsync(new[] { family.CreatedById, family.UpdatedById }, cancellationToken);

        return Ok(ApiResponseFactory.Success(ToDetail(family, students, studentPersons, names), "Family retrieved."));
    }

    #endregion

    #region Update Endpoint

    /// <summary>
    /// Updates an existing family. Family-level fields are patch semantics (only supplied fields
    /// change); the primary contact's <see cref="FamilyPersonMapping"/> row is kept in sync, and a
    /// supplied secondary contact is patched or minted — see <see cref="UpsertSecondaryContactAsync"/>.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.FamiliesWrite)]
    [ProducesResponseType<ApiResponse<FamilyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyRequest request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "An active tenant is required."));
        }

        var family = await LoadAsync(id, cancellationToken);
        if (family is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family not found."));
        }

        // Validated up front, before any mutation starts — mirrors Create's pre-flight checks, so a
        // rejected secondary-contact email never leaves the family partially updated.
        if (request.SecondaryContact is { } secondaryCheck)
        {
            var existingSecondary = family.Contacts.FirstOrDefault(c => !c.Deleted);
            var secondaryEmail = secondaryCheck.Email.Trim();
            var primaryEmailForCompare = request.Email?.Trim() ?? family.Email;
            if (string.Equals(secondaryEmail, primaryEmailForCompare, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponseFactory.Error(
                    ApiErrorCodes.ValidationFailed, "Validation failed.", "The two contacts cannot share the same email."));
            }
            if (!string.Equals(secondaryEmail, existingSecondary?.EmailAddress, StringComparison.OrdinalIgnoreCase)
                && await _users.EmailExistsAsync(secondaryEmail, cancellationToken))
            {
                return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", secondaryEmail));
            }
        }

        // Reject a rename onto an already-taken family name within the tenant.
        if (request.FamilyName is { } newName)
        {
            var trimmed = newName.Trim();
            if (await _families.FamilyNameExistsAsync(family.TenantId, trimmed, family.Id, cancellationToken))
            {
                return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "A family with this name already exists.", trimmed));
            }
            family.FamilyName = trimmed;
        }

        // The primary contact's email lives on its login account first; the Family row's own copy and
        // the linked Person follow it (mirrors StudentsController.Update).
        var primaryPerson = family.PersonId is { } primaryPersonId ? await _persons.GetByIdAsync(primaryPersonId, cancellationToken) : null;
        if (request.Email is { } newEmailRaw)
        {
            var newEmail = newEmailRaw.Trim();
            if (primaryPerson?.UserId is { } linkedUserId)
            {
                var linkedUser = await _users.GetByIdAsync(linkedUserId, cancellationToken);
                if (linkedUser is not null && !string.Equals(newEmail, linkedUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _users.EmailExistsAsync(newEmail, cancellationToken))
                    {
                        return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", newEmail));
                    }
                    linkedUser.Email = newEmail;
                    linkedUser.TokenVersion++; // email change invalidates sessions
                    _users.Update(linkedUser);
                }
            }
            family.Email = newEmail;
        }

        // Patch the primary contact's linked Person (and its login's display name) from the request.
        if (primaryPerson is not null)
        {
            if (request.FirstName is { } fn) { primaryPerson.FirstName = fn.Trim(); family.FirstName = fn.Trim(); }
            if (request.LastName is { } ln) { primaryPerson.LastName = ln.Trim(); family.LastName = ln.Trim(); }
            if (request.FirstName is not null || request.LastName is not null)
            {
                primaryPerson.DisplayName = primaryPerson.FullName;
            }
            primaryPerson.MobileNumber = request.CellPhone?.Trim() ?? primaryPerson.MobileNumber;
            primaryPerson.LastProfileUpdatedOn = DateTime.UtcNow;
            _persons.Update(primaryPerson);

            if (primaryPerson.UserId is { } displayUserId)
            {
                var displayUser = await _users.GetByIdAsync(displayUserId, cancellationToken);
                if (displayUser is not null)
                {
                    displayUser.DisplayName = primaryPerson.FullName;
                    _users.Update(displayUser);
                }
            }
        }

        // Patch the remaining family-level fields (patch semantics: an omitted field keeps its value).
        family.StudioLocationId = request.StudioLocationId ?? family.StudioLocationId;
        family.FamilyStatusId = request.FamilyStatusId ?? family.FamilyStatusId;
        family.Source = request.Source?.Trim() ?? family.Source;
        family.ReferralName = request.ReferralName?.Trim() ?? family.ReferralName;
        family.Active = request.Active;
        family.Type = request.Relation?.Trim() ?? family.Type;
        family.BirthDate = request.BirthDate ?? family.BirthDate;
        family.HomePhone = request.HomePhone?.Trim() ?? family.HomePhone;
        family.WorkPhone = request.WorkPhone?.Trim() ?? family.WorkPhone;
        family.CellPhone = request.CellPhone?.Trim() ?? family.CellPhone;
        family.Fax = request.Fax?.Trim() ?? family.Fax;
        family.OtherPhone = request.OtherPhone?.Trim() ?? family.OtherPhone;
        family.IsBillingContact = request.IsBillingContact;
        family.IsAuthorizedToPickUpStudent = request.IsAuthorizedToPickUpStudent;
        family.Address1 = request.Address1?.Trim() ?? family.Address1;
        family.Address2 = request.Address2?.Trim() ?? family.Address2;
        family.City = request.City?.Trim() ?? family.City;
        family.State = request.State?.Trim() ?? family.State;
        family.ZipCode = request.ZipCode ?? family.ZipCode;
        family.EmergencyContactPerson = request.EmergencyContactPerson?.Trim() ?? family.EmergencyContactPerson;
        family.EmergencyPhone = request.EmergencyPhone?.Trim() ?? family.EmergencyPhone;
        family.HealthInsuranceCarrier = request.HealthInsuranceCarrier?.Trim() ?? family.HealthInsuranceCarrier;
        family.UpdatedOnUtc = DateTime.UtcNow;
        family.UpdatedById = CurrentActorId();
        _families.Update(family);

        // The primary contact's own FamilyPersonMapping row, synced from the (now-merged) Family fields
        // above — unlike the secondary, the primary's Person always already exists (family.PersonId),
        // already updated by the block above, so this only ever finds-or-creates the mapping ROW, never
        // mints a Person.
        var primaryMapping = await SyncPrimaryContactMappingAsync(family, DateTime.UtcNow, CurrentActorId(), cancellationToken);
        var contactResponses = new List<FamilyContactResponse>
        {
            new(primaryMapping.Id, primaryMapping.PersonId, family.FirstName ?? string.Empty, family.LastName ?? string.Empty,
                primaryMapping.EmailAddress, primaryMapping.PhoneNumber, primaryMapping.Relation, true,
                primaryMapping.IsBillingContact, primaryMapping.IsAuthorizedToPickUpStudent, null)
        };

        // A supplied secondary contact is patched/minted; otherwise the existing one (if any) is echoed
        // back unchanged so the response always reflects the family's current contact set.
        if (request.SecondaryContact is { } secondaryInput)
        {
            var response = await UpsertSecondaryContactAsync(family, secondaryInput, cancellationToken);
            contactResponses.Add(response);
        }
        else
        {
            var existing = family.Contacts.FirstOrDefault(c => !c.Deleted && !c.IsPrimaryContact);
            if (existing is not null)
            {
                contactResponses.Add(ToContactResponse(existing));
            }
        }

        await _audit.AddAsync(nameof(Family), family.Id.ToString(), "Updated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new FamilyResponse(family.Id, family.FamilyName, contactResponses), "Family updated."));
    }

    #endregion

    #region Delete Endpoint

    /// <summary>
    /// Soft-deletes a family record. Contact Persons and any enrolled Students are independent records
    /// and are left alone, the same way deleting a Student never deletes its Person.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.FamiliesDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var family = await LoadAsync(id, cancellationToken);
        if (family is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family not found."));
        }

        // Soft delete: flag the row rather than physically removing it.
        family.Deleted = true;
        family.UpdatedOnUtc = DateTime.UtcNow;
        family.UpdatedById = CurrentActorId();
        _families.Update(family);
        await _audit.AddAsync(nameof(Family), family.Id.ToString(), "Deleted", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(new { familyId = id }, "Family deleted."));
    }

    #endregion

    #endregion

    #region Private Helper Methods

    #region LoadAsync Helper

    /// <summary>
    /// Loads a family entity (with its contact mappings), scoped to the caller's active tenant unless
    /// they are a Super Admin, in which case the tenant filter is bypassed.
    /// </summary>
    private Task<Family?> LoadAsync(Guid id, CancellationToken cancellationToken)
        => User.IsSuperAdmin() ? _families.GetByIdUnscopedAsync(id, cancellationToken) : _families.GetByIdAsync(id, cancellationToken);

    #endregion

    #region Contact Minting & Sync Helpers

    /// <summary>Mints a new contact Person + login account (Parent role) — shared by the primary and
    /// secondary contact, each of which also gets its own <see cref="FamilyPersonMapping"/> row (see
    /// that entity's remarks).</summary>
    private async Task<(Person Person, string TemporaryPassword)> MintContactAsync(
        string firstName, string lastName, string email, string? phone, Guid tenantId, Role parentRole,
        DateTime now, CancellationToken cancellationToken)
    {
        var trimmedFirst = firstName.Trim();
        var trimmedLast = lastName.Trim();
        var fullName = string.Join(" ", new[] { trimmedFirst, trimmedLast }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var userId = Guid.NewGuid();

        // The CRM Person master record — there is no existing one to link to for a new contact.
        var person = new Person
        {
            Id = Guid.NewGuid(),
            PersonCode = await GeneratePersonCodeAsync(cancellationToken),
            UserId = userId,
            FirstName = trimmedFirst,
            LastName = trimmedLast,
            DisplayName = fullName,
            PrimaryEmail = email,
            MobileNumber = phone?.Trim(),
            IsActive = true,
            LastProfileUpdatedOn = now,
            SourceEntityType = EntityType.FamilyContact,
        };
        person.TenantMappings.Add(new TenantPersonMapping { Id = Guid.NewGuid(), TenantId = tenantId });
        await _persons.AddAsync(person, cancellationToken);

        // The login account, carrying the Parent role in the active tenant.
        var temporaryPassword = _passwordHasher.GenerateTemporaryPassword();
        var (hash, salt) = _passwordHasher.Hash(temporaryPassword);
        var user = new User
        {
            Id = userId,
            Email = email,
            DisplayName = fullName,
            PersonId = person.Id,
            PasswordHash = hash,
            Salt = salt,
            IsActive = true,
            MustChangePassword = true,
            TokenVersion = 1,
            CreatedDate = now,
        };
        await _users.AddAsync(user, cancellationToken);
        await _users.AddAssignmentAsync(new UserTenantRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            Role = RoleAssignment.MapLegacyRole(parentRole, null),
            RoleId = parentRole.Id,
        }, cancellationToken);

        return (person, temporaryPassword);
    }

    /// <summary>Builds (but does not persist) a <see cref="FamilyPersonMapping"/> row for one contact —
    /// shared shape for both the primary and secondary contact on <see cref="Create"/>.</summary>
    private static FamilyPersonMapping BuildContactMappingRow(
        Guid familyId, Guid personId, string firstName, string lastName, string email, string? phone,
        string? relation, bool isPrimary, bool isBillingContact, bool isAuthorizedToPickUpStudent,
        DateTime now, Guid? actorId) => new()
    {
        Id = Guid.NewGuid(),
        FamilyId = familyId,
        PersonId = personId,
        ContactName = $"{firstName.Trim()} {lastName.Trim()}".Trim(),
        EmailAddress = email.Trim(),
        PhoneNumber = phone?.Trim(),
        Relation = relation?.Trim(),
        IsPrimaryContact = isPrimary,
        IsBillingContact = isBillingContact,
        IsAuthorizedToPickUpStudent = isAuthorizedToPickUpStudent,
        Active = true,
        CreatedOnUtc = now,
        CreatedById = actorId,
        UpdatedOnUtc = now,
        UpdatedById = actorId,
    };

    /// <summary>Finds-or-creates the primary contact's <see cref="FamilyPersonMapping"/> row on
    /// <see cref="Update"/> and syncs it from <paramref name="family"/>'s own (already-merged) fields.
    /// Never mints a Person — the primary's Person always already exists (<see cref="Family.PersonId"/>)
    /// and is updated separately by the caller, unlike <see cref="UpsertSecondaryContactAsync"/>.</summary>
    private async Task<FamilyPersonMapping> SyncPrimaryContactMappingAsync(
        Family family, DateTime now, Guid? actorId, CancellationToken cancellationToken)
    {
        var mapping = family.Contacts.FirstOrDefault(c => !c.Deleted && c.IsPrimaryContact);
        if (mapping is null)
        {
            // A family predating this row's existence for its primary contact — self-heals here.
            mapping = new FamilyPersonMapping
            {
                Id = Guid.NewGuid(),
                FamilyId = family.Id,
                IsPrimaryContact = true,
                Active = true,
                CreatedOnUtc = now,
                CreatedById = actorId,
            };
            await _families.AddContactAsync(mapping, cancellationToken);
            family.Contacts.Add(mapping);
        }

        mapping.PersonId = family.PersonId;
        mapping.ContactName = string.Join(" ", new[] { family.FirstName, family.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        mapping.EmailAddress = family.Email;
        mapping.PhoneNumber = family.CellPhone ?? family.HomePhone;
        mapping.Relation = family.Type;
        mapping.IsBillingContact = family.IsBillingContact;
        mapping.IsAuthorizedToPickUpStudent = family.IsAuthorizedToPickUpStudent;
        mapping.UpdatedOnUtc = now;
        mapping.UpdatedById = actorId;
        _families.UpdateContact(mapping);
        return mapping;
    }

    /// <summary>Patches the family's existing secondary contact, or mints a new one (via
    /// <see cref="MintContactAsync"/>) if it has none yet.</summary>
    private async Task<FamilyContactResponse> UpsertSecondaryContactAsync(
        Family family, FamilyContactInput input, CancellationToken cancellationToken)
    {
        var existing = family.Contacts.FirstOrDefault(c => !c.Deleted && !c.IsPrimaryContact);
        var now = DateTime.UtcNow;
        var actorId = CurrentActorId();

        if (existing is null)
        {
            // Email uniqueness already validated by the caller (Update) before any mutation began.
            var email = input.Email.Trim();
            var parentRole = await _roles.GetByNameAsync(Roles.Parent, cancellationToken)
                ?? throw new InvalidOperationException("The Parent role was not found.");
            var (person, tempPassword) = await MintContactAsync(
                input.FirstName, input.LastName, email, input.Phone, family.TenantId, parentRole, now, cancellationToken);

            var contact = BuildContactMappingRow(
                family.Id, person.Id, input.FirstName, input.LastName, email, input.Phone, input.Relation,
                isPrimary: false, input.IsBillingContact, input.IsAuthorizedToPickUpStudent, now, actorId);
            await _families.AddContactAsync(contact, cancellationToken);
            family.Contacts.Add(contact);

            return new FamilyContactResponse(
                contact.Id, person.Id, input.FirstName.Trim(), input.LastName.Trim(), email, contact.PhoneNumber,
                contact.Relation, false, contact.IsBillingContact, contact.IsAuthorizedToPickUpStudent, tempPassword);
        }

        // Patching an existing secondary contact: its email lives on its login account first, then the
        // linked Person, then the mapping row itself (mirrors the primary contact's own update flow).
        var contactPerson = existing.PersonId is { } pid ? await _persons.GetByIdAsync(pid, cancellationToken) : null;
        var newEmail = input.Email.Trim();
        if (contactPerson is not null)
        {
            if (contactPerson.UserId is { } linkedUserId && !string.Equals(newEmail, contactPerson.PrimaryEmail, StringComparison.OrdinalIgnoreCase))
            {
                var linkedUser = await _users.GetByIdAsync(linkedUserId, cancellationToken);
                if (linkedUser is not null && !string.Equals(newEmail, linkedUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    linkedUser.Email = newEmail;
                    linkedUser.TokenVersion++; // email change invalidates sessions
                    _users.Update(linkedUser);
                }
            }
            contactPerson.FirstName = input.FirstName.Trim();
            contactPerson.LastName = input.LastName.Trim();
            contactPerson.DisplayName = contactPerson.FullName;
            contactPerson.PrimaryEmail = newEmail;
            contactPerson.MobileNumber = input.Phone?.Trim();
            contactPerson.LastProfileUpdatedOn = now;
            _persons.Update(contactPerson);

            if (contactPerson.UserId is { } displayUserId)
            {
                var displayUser = await _users.GetByIdAsync(displayUserId, cancellationToken);
                if (displayUser is not null)
                {
                    displayUser.DisplayName = contactPerson.FullName;
                    _users.Update(displayUser);
                }
            }
        }

        existing.ContactName = $"{input.FirstName.Trim()} {input.LastName.Trim()}".Trim();
        existing.EmailAddress = newEmail;
        existing.PhoneNumber = input.Phone?.Trim();
        existing.Relation = input.Relation?.Trim();
        existing.IsBillingContact = input.IsBillingContact;
        existing.IsAuthorizedToPickUpStudent = input.IsAuthorizedToPickUpStudent;
        existing.UpdatedOnUtc = now;
        existing.UpdatedById = actorId;
        _families.UpdateContact(existing);

        return new FamilyContactResponse(
            existing.Id, existing.PersonId, input.FirstName.Trim(), input.LastName.Trim(), newEmail, existing.PhoneNumber,
            existing.Relation, false, existing.IsBillingContact, existing.IsAuthorizedToPickUpStudent, null);
    }

    /// <summary>Generates a unique business person code (e.g. PER-AB12CD34EF) — mirrors
    /// PersonsController's/StudentsController's generator.</summary>
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

    /// <summary>The current authenticated caller's id, or null when it cannot be parsed as a Guid
    /// (system/background operations).</summary>
    private Guid? CurrentActorId()
        => Guid.TryParse(_actorAccessor.GetCurrentActor(), out var id) ? id : null;

    /// <summary>Batch-resolves user ids to display full names, for Created/Updated By columns.</summary>
    private async Task<IReadOnlyDictionary<Guid, string>> ResolveActorNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        => await _users.GetFullNamesAsync(ids.Where(id => id.HasValue).Select(id => id!.Value), cancellationToken);

    #endregion

    #region Response Mapping Helpers

    /// <summary>Looks up a resolved actor name by id, or null if unresolved/absent.</summary>
    private static string? NameOf(IReadOnlyDictionary<Guid, string> names, Guid? id)
        => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : null;

    /// <summary>Maps a <see cref="FamilyPersonMapping"/> row to its response shape, splitting the
    /// legacy combined <see cref="FamilyPersonMapping.ContactName"/> into first/last for display.</summary>
    private static FamilyContactResponse ToContactResponse(FamilyPersonMapping c) => new(
        c.Id, c.PersonId, FirstNameOf(c.ContactName), LastNameOf(c.ContactName), c.EmailAddress, c.PhoneNumber,
        c.Relation, c.IsPrimaryContact, c.IsBillingContact, c.IsAuthorizedToPickUpStudent, null);

    /// <summary>The first word of a combined "First Last" display name, or empty when blank.</summary>
    private static string FirstNameOf(string? contactName)
        => string.IsNullOrWhiteSpace(contactName) ? string.Empty : contactName.Trim().Split(' ', 2)[0];

    /// <summary>Everything after the first word of a combined "First Last" display name, or empty when
    /// blank or single-word.</summary>
    private static string LastNameOf(string? contactName)
    {
        if (string.IsNullOrWhiteSpace(contactName)) return string.Empty;
        var parts = contactName.Trim().Split(' ', 2);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    /// <summary>The family's contact count — reads from FamilyPersonMapping (the authoritative list, see
    /// its remarks) when it has a primary-contact row, falling back to "1 (Family's own inlined primary)
    /// + however many FamilyPersonMapping rows exist" for a family predating that row existing.</summary>
    private static int ContactCountOf(Family f)
    {
        var active = f.Contacts.Where(c => !c.Deleted).ToList();
        return active.Any(c => c.IsPrimaryContact) ? active.Count : 1 + active.Count;
    }

    /// <summary>Maps a Family entity to its list-row summary shape.</summary>
    private static FamilySummary ToSummary(Family f, IReadOnlyDictionary<Guid, string> names, IReadOnlyDictionary<Guid, int> studentCounts) => new(
        f.Id, f.FamilyName,
        string.Join(" ", new[] { f.FirstName, f.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))),
        f.Email, f.CellPhone ?? f.HomePhone,
        ContactCountOf(f),
        studentCounts.TryGetValue(f.Id, out var count) ? count : 0,
        f.StudioLocationId, f.StudioLocation?.Name,
        f.FamilyStatusId, f.FamilyStatus?.Name,
        f.Active,
        NameOf(names, f.CreatedById), f.CreatedOnUtc, NameOf(names, f.UpdatedById), f.UpdatedOnUtc);

    /// <summary>Maps a Family entity (with its loaded contacts/students) to the full detail shape
    /// returned by <see cref="GetById"/>.</summary>
    private static FamilyDetail ToDetail(
        Family f, IReadOnlyList<Student> students, IReadOnlyDictionary<Guid, Person> studentPersons, IReadOnlyDictionary<Guid, string> names)
    {
        // FamilyPersonMapping is the authoritative list of contacts (see its remarks) — both primary and
        // secondary now live there. The synthetic fallback below only fires for a family predating that
        // row existing for its primary contact.
        var active = f.Contacts.Where(c => !c.Deleted).OrderByDescending(c => c.IsPrimaryContact).ToList();
        var contacts = active.Select(ToContactResponse).ToList();
        if (!active.Any(c => c.IsPrimaryContact))
        {
            contacts.Insert(0, new(null, f.PersonId, f.FirstName ?? string.Empty, f.LastName ?? string.Empty, f.Email, f.CellPhone ?? f.HomePhone,
                f.Type, true, f.IsBillingContact, f.IsAuthorizedToPickUpStudent, null));
        }

        return new FamilyDetail(
            f.Id, f.FamilyName, f.StudioLocationId, f.StudioLocation?.Name, f.FamilyStatusId, f.FamilyStatus?.Name,
            f.Source, f.ReferralName, f.HomePhone, f.WorkPhone, f.Fax, f.OtherPhone,
            f.Address1, f.Address2, f.City, f.State, f.ZipCode,
            f.EmergencyContactPerson, f.EmergencyPhone, f.HealthInsuranceCarrier, f.Active,
            contacts,
            students.Select(s =>
            {
                var person = s.PersonId is { } pid && studentPersons.TryGetValue(pid, out var p) ? p : null;
                return new FamilyStudentSummary(s.Id, person?.FirstName, person?.LastName, s.StudentNumber, s.Active, s.ClassId);
            }).ToList(),
            NameOf(names, f.CreatedById), f.CreatedOnUtc, NameOf(names, f.UpdatedById), f.UpdatedOnUtc);
    }

    #endregion

    #endregion
}
