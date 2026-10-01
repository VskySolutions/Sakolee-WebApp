using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.EPaymentSchedule;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for E-Payment Schedule records.
/// E-Payment Schedule records are scoped to the caller's active tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/e-payment-schedules")]
[Produces("application/json")]
[Tags("E-Payment Schedule")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class EPaymentScheduleController : ControllerBase
{
    #region Fields

    private readonly IEPaymentScheduleRepository _ePaymentSchedules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the E-Payment Schedule controller.
    /// </summary>
    public EPaymentScheduleController(IEPaymentScheduleRepository ePaymentSchedules,IUnitOfWork unitOfWork,IUserRepository users)
    {
        _ePaymentSchedules = ePaymentSchedules;
        _unitOfWork = unitOfWork;
        _users = users;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets all non-deleted E-Payment Schedule records for the caller's active tenant.
    /// Supports searching and sorting by name and audit dates.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.EPaymentSchedulesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? name = null,[FromQuery] bool showDeleted = false,[FromQuery] bool? active = null,[FromQuery] string? search = null,[FromQuery] string? sortBy = null,[FromQuery] bool descending = false,[FromQuery] int page = 1,[FromQuery] int limit = 20,CancellationToken cancellationToken = default)
    {
        // Get the active tenant ID from the authenticated user's claims.
        // The request cannot continue if no active tenant is available.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Make sure the page number is at least 1.
        page = Math.Max(1, page);
        // Restrict the page size between 1 and 100 records.
        limit = Math.Clamp(limit, 1, 100);
        // Retrieve the E-Payment Schedule records from the repository
        // using the requested filters, search, sorting, and pagination.
        var (ePaymentSchedules, total) =await _ePaymentSchedules.ListByTenantAsync(tenantId,name,showDeleted,active,search,sortBy,descending,page,limit,cancellationToken);
        // Get the display names of users referenced by CreatedById
        // and UpdatedById audit fields.
        var nameOf = await AuditNamesAsync(ePaymentSchedules,cancellationToken);
        // Convert the database entities into API response models.
        var summaries = ePaymentSchedules.Select(x => ToSummary(x, nameOf)).ToList();
        // Return the records together with pagination information.
        return Ok(ApiResponseFactory.Paginated(summaries,"E-Payment Schedule records retrieved.",page,limit,total));
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets an E-Payment Schedule record by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.EPaymentSchedulesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID to ensure that the requested
        // record belongs to the caller's tenant.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve the E-Payment Schedule record using its ID
        // and the active tenant ID.
        var ePaymentSchedule =await _ePaymentSchedules.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the requested record does not exist.
        if (ePaymentSchedule is null)
        {
            return NotFound(ApiResponseFactory.NotFound("E-Payment Schedule record not found."));
        }
        // Resolve the display names for the audit user IDs.
        var nameOf = await AuditNamesAsync(new[] { ePaymentSchedule },cancellationToken);
        // Convert the entity into the API response model.
        var summary = ToSummary(ePaymentSchedule,nameOf);
        // Return the requested record.
        return Ok(ApiResponseFactory.Success(summary,"E-Payment Schedule record retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new E-Payment Schedule record for the caller's active tenant.
    /// The name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.EPaymentSchedulesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEPaymentScheduleRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID for the new record.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the E-Payment Schedule name was provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "E-Payment Schedule name is required." });
        }
        // Remove leading and trailing whitespace from the name
        // before saving and checking for duplicates.
        var name = request.Name.Trim();
        // Check whether another non-deleted record with the same
        // name already exists for this tenant.
        var nameExists =await _ePaymentSchedules.ExistsByNameAsync(tenantId,name,cancellationToken: cancellationToken);
        // Return a conflict response when the name already exists.
        if (nameExists)
        {
            return Conflict(new{message =$"E-Payment Schedule record with the name '{name}' already exists."});
        }
        // Create a new E-Payment Schedule entity.
        var ePaymentSchedule = new Domain.Entities.EPaymentSchedule{ Id = Guid.NewGuid(),TenantId = tenantId, Name = name, Active = request.Active };
        // Add the new entity to the current DbContext.
        await _ePaymentSchedules.AddAsync(ePaymentSchedule,cancellationToken);
        // Save the new record to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve the newly created record again so that
        // its complete data, including audit information, is available.
        var createdEPaymentSchedule =await _ePaymentSchedules.GetByIdAsync(ePaymentSchedule.Id,tenantId,cancellationToken);
        // Use the reloaded entity when available.
        // Fall back to the newly created entity if it could not be reloaded.
        var created =createdEPaymentSchedule ?? ePaymentSchedule;
        // Resolve the names of the users stored in the audit fields.
        var nameOf = await AuditNamesAsync(new[] { created }, cancellationToken);
        // Convert the created entity into the API response model.
        var summary = ToSummary(created,nameOf);
        // Return the newly created record with HTTP 201.
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary,"E-Payment Schedule record created."));
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing E-Payment Schedule record.
    /// The name must be unique within the tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.EPaymentSchedulesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateEPaymentScheduleRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID to ensure that the update
        // is performed within the caller's tenant.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the E-Payment Schedule name was provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "E-Payment Schedule name is required." });
        }
        // Trim whitespace before checking and updating the name.
        var name = request.Name.Trim();
        // Retrieve the existing record from the current tenant.
        var ePaymentSchedule =await _ePaymentSchedules.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the record does not exist.
        if (ePaymentSchedule is null)
        {
            return NotFound(ApiResponseFactory.NotFound("E-Payment Schedule record not found."));
        }
        // Check whether another record already uses the requested name.
        // The current record is excluded from this duplicate check.
        var nameExists =await _ePaymentSchedules.ExistsByNameAsync(tenantId,name, excludeId: id,cancellationToken: cancellationToken);
        // Return a conflict response when the name is already in use.
        if (nameExists)
        {
            return Conflict(new { message = $"An E-Payment Schedule record with the name '{name}' already exists." });
        }
        // Update the editable properties of the entity.
        ePaymentSchedule.Name = name;
        ePaymentSchedule.Active = request.Active;
        // Mark the entity as modified in the DbContext.
        _ePaymentSchedules.Update(ePaymentSchedule);
        // Save the changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Resolve the audit user display names.
        var nameOf = await AuditNamesAsync(new[] { ePaymentSchedule },cancellationToken);
        // Convert the updated entity into the API response model.
        var summary = ToSummary(ePaymentSchedule,nameOf);
        // Return the updated record.
        return Ok(ApiResponseFactory.Success(summary,"E-Payment Schedule record updated."));
    }

    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes an E-Payment Schedule record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.EPaymentSchedulesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID to ensure that the delete
        // operation is restricted to the caller's tenant.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve the record that should be deleted.
        var ePaymentSchedule =await _ePaymentSchedules.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the record does not exist.
        if (ePaymentSchedule is null)
        {
            return NotFound(ApiResponseFactory.NotFound("E-Payment Schedule record not found."));
        }
        // Mark the record as deleted instead of physically
        // removing it from the database.
        ePaymentSchedule.Deleted = true;
        // Store the UTC date and time when the record was deleted.
        ePaymentSchedule.DeletedOnUtc = DateTime.UtcNow;
        // Mark the entity as modified.
        _ePaymentSchedules.Update(ePaymentSchedule);
        // Save the soft-delete changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Return the ID of the deleted record.
        return Ok(ApiResponseFactory.Success(new { ePaymentScheduleId = ePaymentSchedule.Id },"E-Payment Schedule record deleted."));
    }

    #endregion

    #region Summary

    /// <summary>
    /// Maps an E-Payment Schedule entity to its API response summary.
    /// </summary>
    private static EPaymentScheduleSummary ToSummary(Domain.Entities.EPaymentSchedule ePaymentSchedule,Func<Guid?, string?> nameOf)
    {
        // Create the API summary using the entity's values
        // and resolve the audit user IDs into display names.
        return new EPaymentScheduleSummary(ePaymentSchedule.Id,ePaymentSchedule.Name,ePaymentSchedule.Active,ePaymentSchedule.Deleted,ePaymentSchedule.TenantId,ePaymentSchedule.Tenant?.Name ?? string.Empty,nameOf(ePaymentSchedule.CreatedById),ePaymentSchedule.CreatedOnUtc,nameOf(ePaymentSchedule.UpdatedById),ePaymentSchedule.UpdatedOnUtc);
    }

    #endregion

    #region Audit Names

    /// <summary>
    /// Retrieves display names for users referenced by the audit fields.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Domain.Entities.EPaymentSchedule> rows,CancellationToken cancellationToken)
    {
        // Collect the CreatedById and UpdatedById values
        // from all returned E-Payment Schedule records.
        var ids = rows.SelectMany(x => new[]{ x.CreatedById,x.UpdatedById }).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        // Retrieve the full names of all referenced users in one call.
        var names =await _users.GetFullNamesAsync(ids,cancellationToken);
        // Return a function that can convert a user ID
        // into its corresponding display name.
        // Return null when the user ID is not available or not found.
        return id =>id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    #endregion
}
