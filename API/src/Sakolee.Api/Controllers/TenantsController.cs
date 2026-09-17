using Sakolee.Api.Models;
using Sakolee.Api.Models.Tenants;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
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
    private readonly IOptionSetRepository _optionSets;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    public TenantsController(
        ITenantRepository tenants,
        IUserRepository users,
        IOptionSetRepository optionSets,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _tenants = tenants;
        _users = users;
        _optionSets = optionSets;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region Tenant Lifecycle (Super Admin)

    /// <summary>
    /// Creates a new tenant with a unique <see cref="CreateTenantRequest.Identifier"/>, seeds it with its
    /// own copy of the platform's default option lists (so its admins can manage values independently of
    /// the shared originals), and starts it in <see cref="TenantStatus.Active"/>.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.TenantsWrite)]
    [ProducesResponseType<ApiResponse<TenantResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        if (await _tenants.IdentifierExistsAsync(request.Identifier, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Identifier already in use.", request.Identifier));
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Identifier = request.Identifier,
            TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "UTC" : request.TimeZoneId,
            Status = TenantStatus.Active,
            CreatedDate = DateTime.UtcNow,
        };
        await _tenants.AddAsync(tenant, cancellationToken);

        // The new tenant gets its OWN copy of the platform's default option lists, so its admins can manage
        // the values (add / rename / delete / re-order) without touching the shared originals.
        await TenantOptionSetSeeder.EnsureDefaultsAsync(_optionSets, tenant.Id, cancellationToken);

        await _audit.AddAsync(nameof(Tenant), tenant.Id.ToString(), "Created", details: tenant.Identifier, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(new TenantResponse(tenant.Id, tenant.Identifier, tenant.Status.ToString()), "Tenant created."));
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
        _tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(
            new TenantResponse(tenant.Id, tenant.Identifier, tenant.Status.ToString()), "Tenant updated."));
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

    #endregion
}
