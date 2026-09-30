using Sakolee.Api.Models;
using Sakolee.Api.Models.FamilyStatus;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Controller managing family status master records for multi-tenant configurations.
/// </summary>
[ApiController]
[Route("/api/admin/family-statuses")]
[Produces("application/json")]
[Tags("Family Statuses")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class FamilyStatusesController : ControllerBase
{
    #region Field Declarations

    private readonly IFamilyStatusRepository _familyStatuses;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _audit;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyStatusesController"/> class.
    /// </summary>
    public FamilyStatusesController(
        IFamilyStatusRepository familyStatuses,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IAuditTrailService audit)
    {
        _familyStatuses = familyStatuses;
        _users = users;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    #endregion

    #region API Endpoints

    #region Create Endpoint

    /// <summary>
    /// Creates a new family status record.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.FamilyStatusesWrite)]
    [ProducesResponseType<ApiResponse<FamilyStatusDetail>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFamilyStatusRequest request, CancellationToken cancellationToken)
    {
        // Validate if the request body name is null or whitespace
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "Family status name is required."));
        }

        // Check if a family status with the exact same name already exists in the system
        var trimmedName = request.Name.Trim();

        if (await _familyStatuses.ExistsAsync(trimmedName, null, cancellationToken))
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "A family status with this name already exists."));
        }

      

        // Retrieve and validate the active tenant identifier from the current user context
        var tenantId = User.GetActiveTenantId() ?? Guid.Empty;
        if (tenantId == Guid.Empty)
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "Tenant ID is required."));
        }

        // Initialize a new FamilyStatus entity; CreatedById/CreatedOnUtc are stamped by the DbContext
        // (AuditableEntity).
        var familyStatus = new FamilyStatus
        {
            FamilyStatusId = Guid.NewGuid(),
            TenantId = tenantId,
            Name = trimmedName,
            Active = request.Active,
        };

        // Persist the new record to the database via repository and unit of work
        await _familyStatuses.AddAsync(familyStatus, cancellationToken);
        await _audit.AddAsync(nameof(FamilyStatus), familyStatus.FamilyStatusId.ToString(), "Created", details: familyStatus.Name, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map the created entity to a detailed response DTO
        var detail = await ToDetailAsync(familyStatus, cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(detail, "Family status created."));
    }

    #endregion

    #region List Endpoint

    /// <summary>
    /// Retrieves a paginated list of family status records. Also readable with families.read: the
    /// Family form loads this list for its status dropdown.
    /// </summary>
    [HttpGet]
    [RequireAnyPermission(Permissions.FamilyStatusesRead, Permissions.FamiliesRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination boundary values
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        // Scope tenant filter for super admin users if explicitly specified in query parameters
        Guid? scopeTenant = User.IsSuperAdmin() && tenantId is { } tid ? tid : null;

        // Fetch filtered, sorted, and paginated records from the repository
        var (items, total) = await _familyStatuses.ListAsync(
            search, scopeTenant, isActive, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);



        // Resolve the audit user IDs into display names for the Created By / Updated By columns
        var nameOf = await AuditNamesAsync(items, cancellationToken);

        // Project database model items into summary DTOs including Tenant information for client consumption
        var summaries = items.Select(f => new FamilyStatusSummary(
            f.FamilyStatusId,
            f.Name,
            f.Active,
            nameOf(f.CreatedById),
            nameOf(f.UpdatedById),
            f.CreatedOnUtc,
            f.UpdatedOnUtc,
            f.TenantId,
            f.Tenant?.Name
        ));

        return Ok(ApiResponseFactory.Paginated(summaries, "Family statuses retrieved.", page, limit, total));
    }

    #endregion

    #region GetById Endpoint

    /// <summary>
    /// Retrieves a specific family status record by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.FamilyStatusesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Load the family status entity handling tenant scoping rules
        var familyStatus = await LoadAsync(id, cancellationToken);
        if (familyStatus is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family status not found."));
        }

        // Map entity details to response DTO
        var detail = await ToDetailAsync(familyStatus, cancellationToken);

        return Ok(ApiResponseFactory.Success(detail, "Family status retrieved."));
    }

    #endregion

    #region Update Endpoint

    /// <summary>
    /// Updates an existing family status record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.FamilyStatusesWrite)]

    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyStatusRequest request, CancellationToken cancellationToken)
    {
        var familyStatus = await LoadAsync(id, cancellationToken);
        if (familyStatus is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family status not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var trimmedName = request.Name.Trim();

            if (await _familyStatuses.ExistsAsync(trimmedName, id, cancellationToken))
            {
                return BadRequest(ApiResponseFactory.Error(
                    ApiErrorCodes.ValidationFailed, "Validation failed.", "A family status with this name already exists."));
            }

            familyStatus.Name = trimmedName;
        }

        if (request.Active.HasValue)
        {
            familyStatus.Active = request.Active.Value;
        }

        // UpdatedById/UpdatedOnUtc are stamped by the DbContext (AuditableEntity).
        _familyStatuses.Update(familyStatus);

        await _audit.AddAsync(nameof(FamilyStatus), familyStatus.FamilyStatusId.ToString(), "Updated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = await ToDetailAsync(familyStatus, cancellationToken);

        return Ok(ApiResponseFactory.Success(detail, "Family status updated."));
    }



    #endregion

    #region Delete Endpoint

    /// <summary>
    /// Soft deletes a family status record (the DbContext turns the remove into Deleted = true).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.FamilyStatusesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // Verify delete permissions if the caller is not a super admin
        if (!User.IsSuperAdmin() && !User.HasPermission(Permissions.FamilyStatusesDelete))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponseFactory.Forbidden("You do not have permission to delete family statuses."));
        }

        // Load the target family status entity
        var familyStatus = await LoadAsync(id, cancellationToken);
        if (familyStatus is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family status not found."));
        }

        // Soft delete: StampAudit converts the remove into Deleted/DeletedOnUtc + Updated* stamps.
        _familyStatuses.Remove(familyStatus);
        await _audit.AddAsync(nameof(FamilyStatus), familyStatus.FamilyStatusId.ToString(), "SoftDeleted", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id }, "Family status moved to trash successfully."));
    }

    #endregion

    #endregion

    #region Private Helper Methods

    #region LoadAsync Helper

    /// <summary>
    /// Loads a family status entity based on super admin privileges or standard tenant context filters.
    /// </summary>
    private Task<FamilyStatus?> LoadAsync(Guid id, CancellationToken cancellationToken)
        => User.IsSuperAdmin()
            ? _familyStatuses.GetByIdUnscopedAsync(id, cancellationToken)
            : _familyStatuses.GetByIdAsync(id, cancellationToken);

    #endregion

    #region Audit Name Helpers

    /// <summary>
    /// Resolves the user IDs stored in the audit fields into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<FamilyStatus> rows, CancellationToken cancellationToken)
    {
        var ids = rows
            .SelectMany(f => new[] { f.CreatedById, f.UpdatedById })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct();

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    /// <summary>
    /// Maps a family status entity to its detail response with audit names resolved.
    /// </summary>
    private async Task<FamilyStatusDetail> ToDetailAsync(FamilyStatus familyStatus, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { familyStatus }, cancellationToken);

        return new FamilyStatusDetail(
            familyStatus.FamilyStatusId,
            familyStatus.Name,
            familyStatus.Active,
            nameOf(familyStatus.CreatedById),
            nameOf(familyStatus.UpdatedById),
            familyStatus.CreatedOnUtc,
            familyStatus.UpdatedOnUtc);
    }

    #endregion

    #endregion
}