using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models.HearAboutUs;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides administrative operations for Hear About Us records.
/// Hear About Us records are scoped to the caller's active tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/hear-about-us")]
[Produces("application/json")]
[Tags("Hear About Us")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class HearAboutUsController : ControllerBase
{
    #region Fields

    // Repository used to perform Hear About Us database operations.
    private readonly IHearAboutUsRepository _hearAboutUs;

    // Unit of Work used to save database changes.
    private readonly IUnitOfWork _unitOfWork;

    // Repository used to retrieve user names for audit information.
    private readonly IUserRepository _users;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Hear About Us controller.
    /// </summary>
    public HearAboutUsController(IHearAboutUsRepository hearAboutUs, IUnitOfWork unitOfWork, IUserRepository users)
    {
        _hearAboutUs = hearAboutUs;
        _unitOfWork = unitOfWork;
        _users = users;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets all non-deleted Hear About Us records for the caller's active tenant.
    /// Supports searching and sorting by name and audit dates.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.HearAboutUsRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? name = null,[FromQuery] bool showDeleted = false, [FromQuery] bool? active = null, [FromQuery] string? search = null,[FromQuery] string? sortBy = null,[FromQuery] bool descending = false,[FromQuery] int page = 1,[FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        // Get the active tenant ID of the currently logged-in user.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);
        // Get Hear About Us records for the active tenant.
        var (hearAboutUs, total) = await _hearAboutUs.ListByTenantAsync(tenantId, name,showDeleted, active, search,sortBy,descending, page,limit, cancellationToken);
        // Get the names of users who created or updated the records.
        var nameOf = await AuditNamesAsync(hearAboutUs,cancellationToken);
        // Map entities to response summaries.
        var summaries = hearAboutUs.Select(x => ToSummary(x, nameOf)).ToList();
        return Ok(ApiResponseFactory.Paginated(summaries,"Hear About Us records retrieved.",page,limit,total));
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets a Hear About Us record by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.HearAboutUsRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Get the Hear About Us record for the active tenant.
        var hearAboutUs = await _hearAboutUs.GetByIdAsync(id,tenantId,cancellationToken);
        if (hearAboutUs is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Hear About Us record not found."));
        }
        // Get audit user names.
        var nameOf = await AuditNamesAsync(new[] { hearAboutUs },cancellationToken);
        // Map the entity to the response.
        var summary = ToSummary(hearAboutUs,nameOf);
        return Ok(ApiResponseFactory.Success(summary,"Hear About Us record retrieved."));
    }

    #endregion

    #region Create

    /// <summary>
    /// Creates a new Hear About Us record for the caller's active tenant.
    /// The name must be unique within the tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.HearAboutUsWrite)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateHearAboutUsRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Validate the Hear About Us name.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Hear About Us name is required." });
        }
        // Remove leading and trailing spaces.
        var name = request.Name.Trim();
        // Check for duplicate active records within the tenant.
        var nameExists = await _hearAboutUs.ExistsByNameAsync(tenantId, name, cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Conflict(new { message = $"Hear About Us record with the name '{name}' already exists."});
        }
        // Create the new entity.
        var hearAboutUs = new Domain.Entities.HearAboutUs
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Active = request.Active
        };
        // Add the entity to the DbContext.
        await _hearAboutUs.AddAsync(hearAboutUs, cancellationToken);
        // Save the new record.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Retrieve the created record with tenant information.
        var createdHearAboutUs = await _hearAboutUs.GetByIdAsync( hearAboutUs.Id, tenantId,cancellationToken);
        // Use the retrieved entity if available.
        var created = createdHearAboutUs ?? hearAboutUs;
        // Get audit user names.
        var nameOf = await AuditNamesAsync(new[] { created },cancellationToken);
        // Map the created entity to the response.
        var summary = ToSummary(created,nameOf);
        return StatusCode(StatusCodes.Status201Created,ApiResponseFactory.Success(summary, "Hear About Us record created."));
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates an existing Hear About Us record.
    /// The name must be unique within the tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.HearAboutUsWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateHearAboutUsRequest request,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }

        // Validate the Hear About Us name.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Hear About Us name is required." });
        }
        // Remove leading and trailing spaces.
        var name = request.Name.Trim();
        // Get the existing record.
        var hearAboutUs = await _hearAboutUs.GetByIdAsync(id,tenantId,cancellationToken);
        if (hearAboutUs is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Hear About Us record not found."));
        }
        // Check whether another record already has the same name.
        var nameExists = await _hearAboutUs.ExistsByNameAsync(tenantId, name, excludeId: id, cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Conflict(new { message = $"A Hear About Us record with the name '{name}' already exists." });
        }
        // Update the record.
        hearAboutUs.Name = name;
        hearAboutUs.Active = request.Active;
        // Mark the entity as modified.
        _hearAboutUs.Update(hearAboutUs);
        // Save the changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Get audit user names.
        var nameOf = await AuditNamesAsync(new[] { hearAboutUs },cancellationToken);
        // Map the updated record to the response.
        var summary = ToSummary(hearAboutUs, nameOf);
        return Ok(ApiResponseFactory.Success(summary,"Hear About Us record updated."));
    }

    #endregion

    #region Delete

    /// <summary>
    /// Soft deletes a Hear About Us record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.HearAboutUsDelete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        // Get the active tenant ID of the currently logged-in user.
        if (User.GetActiveTenantId() is not { } tenantId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,ApiResponseFactory.Forbidden("No active tenant for the caller."));
        }
        // Get the record for the active tenant.
        var hearAboutUs = await _hearAboutUs.GetByIdAsync(id,tenantId,cancellationToken);
        if (hearAboutUs is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Hear About Us record not found."));
        }
        // Mark the record as deleted.
        hearAboutUs.Deleted = true;
        hearAboutUs.DeletedOnUtc = DateTime.UtcNow;
        // Mark the entity as modified.
        _hearAboutUs.Update(hearAboutUs);
        // Save the changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(new{ hearAboutUsId = hearAboutUs.Id }, "Hear About Us record deleted."));
    }

    #endregion

    #region Summary

    /// <summary>
    /// Maps a Hear About Us entity to its API response summary.
    /// </summary>
    private static HearAboutUsSummary ToSummary(Domain.Entities.HearAboutUs hearAboutUs,Func<Guid?, string?> nameOf)
    {
        return new HearAboutUsSummary(hearAboutUs.Id,hearAboutUs.Name, hearAboutUs.Active, hearAboutUs.Deleted, hearAboutUs.TenantId, hearAboutUs.Tenant?.Name ?? string.Empty, nameOf(hearAboutUs.CreatedById),hearAboutUs.CreatedOnUtc,nameOf(hearAboutUs.UpdatedById), hearAboutUs.UpdatedOnUtc);
    }

    #endregion

    #region Audit Names

    /// <summary>
    /// Retrieves display names for users referenced by the audit fields.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Domain.Entities.HearAboutUs> rows,CancellationToken cancellationToken)
    {
        // Collect CreatedBy and UpdatedBy user IDs.
        var ids = rows.SelectMany(x => new[] { x.CreatedById, x.UpdatedById }).Where(id => id.HasValue) .Select(id => id!.Value).Distinct();
        // Retrieve the corresponding user names.
        var names = await _users.GetFullNamesAsync(ids,cancellationToken);
        // Return a function that resolves user IDs to display names.
        return id =>id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    #endregion
}