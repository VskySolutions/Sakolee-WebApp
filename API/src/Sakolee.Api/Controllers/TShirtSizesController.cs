using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.TShirtSizes;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Sakolee.Application.Abstractions.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for T-Shirt Size records.
/// T-Shirt Sizes are scoped to the caller's active tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/t-shirt-sizes")]
[Produces("application/json")]
[Tags("T-Shirt Sizes")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class TShirtSizesController : ControllerBase
{
    #region Fields
    private readonly ITShirtSizeRepository _tShirtSizes;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes the T-Shirt Sizes controller.
    /// </summary>
    public TShirtSizesController(ITShirtSizeRepository tShirtSizes, IUserRepository users, IUnitOfWork unitOfWork)
    {
        _tShirtSizes = tShirtSizes;
        _users = users;
        _unitOfWork = unitOfWork;
    }
    #endregion

    #region List
    /// <summary>
    /// Gets all non-deleted T-Shirt Sizes for the caller's active tenant.
    /// Supports searching by T-Shirt Size name.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.TShirtSizesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search = null,CancellationToken cancellationToken = default)
    {
        // Get the active tenant ID from the currently logged-in user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Retrieve all non-deleted T-Shirt Sizes belonging to the active tenant.
        // The search value is passed to the repository when provided.
        var tShirtSizes = await _tShirtSizes.ListByTenantAsync(tenantId,search,cancellationToken);
        // Resolve the Created By and Updated By user IDs into display names.This avoids returning only user IDs in the API response.
        var nameOf = await AuditNamesAsync(tShirtSizes, cancellationToken);
        // Convert each T-Shirt Size entity into the response summary model.
        var summaries = tShirtSizes.Select(x => ToSummary(x, nameOf)).ToList();
        // Return the T-Shirt Size summaries in the standard API response format.
        return Ok(ApiResponseFactory.Success(summaries,"T-Shirt sizes retrieved."));
    }
    #endregion

    #region Get

    /// <summary>
    /// Gets a T-Shirt Size by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.TShirtSizesRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Find the requested T-Shirt Size within the active tenant.
        var tShirtSize = await _tShirtSizes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the requested record does not exist or has already been deleted.
        if (tShirtSize is null)
        {
            return NotFound(ApiResponseFactory.NotFound("T-Shirt size not found."));
        }
        // Convert the entity into the response model.
        var summary = ToSummary(tShirtSize,await AuditNamesAsync(new[] { tShirtSize }, cancellationToken));        
        // Return the requested T-Shirt Size.
        return Ok(ApiResponseFactory.Success(summary,"T-Shirt size retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new T-Shirt Size for the caller's active tenant.
    /// The T-Shirt Size name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.TShirtSizesWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTShirtSizeRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID from the logged-in user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the T-Shirt Size name has been provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "T-Shirt size name is required." });
        }
        // Remove unnecessary spaces from the beginning and end of the name.
        var name = request.Name.Trim();
        // Check whether another active T-Shirt Size with the same name already exists for the current tenant.
        var nameExists = await _tShirtSizes.ExistsByNameAsync(tenantId, name, cancellationToken: cancellationToken);
        // Prevent duplicate T-Shirt Size names within the same tenant.
        if (nameExists)
        {
            return Conflict(new { message = $"A T-Shirt size with the name '{name}' already exists." });
        }
        // Create a new T-Shirt Size entity with tenant and audit information.
        var tShirtSize = new Domain.Entities.TShirtSize { Id = Guid.NewGuid(),TenantId = tenantId, Name = name, CreatedOnUtc = DateTime.UtcNow, CreatedById = User.GetUserId(), Deleted = false };
        // Add the new entity to the current DbContext.
        await _tShirtSizes.AddAsync(tShirtSize,cancellationToken);
        // Save the new record to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve the newly created record again so that related tenant information is available in the response.
        var createdTShirtSize = await _tShirtSizes.GetByIdAsync(tShirtSize.Id,tenantId,cancellationToken);
        // Create the response model for the newly created record.
        var nameOf = await AuditNamesAsync(new[] { createdTShirtSize ?? tShirtSize },cancellationToken);
        var summary = ToSummary(createdTShirtSize ?? tShirtSize,nameOf);        
        // Return HTTP 201 Created with the created T-Shirt Size.
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary,"T-Shirt size created."));
    }
    #endregion

    #region Update

    /// <summary>
    /// Updates an existing T-Shirt Size.
    /// The T-Shirt Size name must be unique within the tenant,
    /// excluding the T-Shirt Size currently being updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.TShirtSizesWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateTShirtSizeRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate that the updated T-Shirt Size name is provided.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "T-Shirt size name is required." });
        }
        // Remove unnecessary spaces from the provided name.
        var name = request.Name.Trim();
        // Find the existing T-Shirt Size within the active tenant.
        var tShirtSize = await _tShirtSizes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the requested T-Shirt Size does not exist.
        if (tShirtSize is null)
        {
            return NotFound(ApiResponseFactory.NotFound("T-Shirt size not found."));
        }
        // Check whether another T-Shirt Size already uses the new name.
        // The current record is excluded from this duplicate check.
        var nameExists = await _tShirtSizes.ExistsByNameAsync(tenantId,name,excludeId: id,cancellationToken: cancellationToken);
        // Prevent duplicate names within the same tenant.
        if (nameExists)
        {
            return Conflict(new { message = $"A T-Shirt size with the name '{name}' already exists."});
        }
        // Update the T-Shirt Size name.
        tShirtSize.Name = name;
        // Update audit information for the modification.
        tShirtSize.UpdatedOnUtc = DateTime.UtcNow;
        tShirtSize.UpdatedById = User.GetUserId();
        // Mark the entity as modified in the current DbContext.
        _tShirtSizes.Update(tShirtSize);
        // Save the updated record to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Create the response model containing the updated information.
        var nameOf = await AuditNamesAsync(new[] { tShirtSize },cancellationToken);
        var summary = ToSummary(tShirtSize,nameOf);        
        // Return the updated T-Shirt Size.
        return Ok(ApiResponseFactory.Success(summary,"T-Shirt size updated."));
    }

    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes a T-Shirt Size.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.TShirtSizesDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID from the current user's claims.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Find the T-Shirt Size that belongs to the active tenant.
        var tShirtSize = await _tShirtSizes.GetByIdAsync(id,tenantId,cancellationToken);
        // Return 404 when the requested record does not exist.
        if (tShirtSize is null)
        {
            return NotFound(ApiResponseFactory.NotFound("T-Shirt size not found."));
        }
        // Mark the record as deleted instead of physically removing it.
        tShirtSize.Deleted = true;
        // Update audit information for the deletion.
        tShirtSize.UpdatedOnUtc = DateTime.UtcNow;
        tShirtSize.UpdatedById = User.GetUserId();
        // Mark the entity as modified in the current DbContext.
        _tShirtSizes.Update(tShirtSize);
        // Save the soft-delete changes to the database.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Return the identifier of the deleted T-Shirt Size.
        return Ok(ApiResponseFactory.Success(new { tShirtSizeId = tShirtSize.Id }, "T-Shirt size deleted."));
    }

    #endregion

    #region Summary

    /// <summary>
    /// Converts a T-Shirt Size entity into a summary response model.
    /// Resolves the Created By and Updated By user IDs into display names.
    /// </summary>
    private static TShirtSizeSummary ToSummary(Domain.Entities.TShirtSize tShirtSize,Func<Guid?, string?> nameOf)
    {
        // Create the API response model and resolve the audit user names.
        return new TShirtSizeSummary(tShirtSize.Id,tShirtSize.Name,tShirtSize.TenantId,tShirtSize.Tenant?.Name ?? string.Empty,nameOf(tShirtSize.CreatedById),tShirtSize.CreatedOnUtc,nameOf(tShirtSize.UpdatedById),tShirtSize.UpdatedOnUtc);
    }

    #endregion

    #region Audit Names

    /// <summary>
    /// Resolves the Created By and Updated By user IDs for the specified
    /// T-Shirt Size records into user display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Domain.Entities.TShirtSize> rows, CancellationToken cancellationToken)
    {
        // Collect all Created By and Updated By user IDs.
        var ids = rows.SelectMany(x => new[] { x.CreatedById, x.UpdatedById }).Where(id => id.HasValue).Select(id => id!.Value).Distinct();
        // Retrieve the user names for the collected IDs.
        var names = await _users.GetFullNamesAsync(ids, cancellationToken);
        // Return a function that resolves a user ID to its display name.
        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }
    #endregion
}