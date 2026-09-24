using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.Locations;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Location Master management for the active tenant.
/// Locations belong to the Dance Studio/Tenant that owns them.
/// </summary>
[ApiController]
[Route("/api/admin/locations")]
[Produces("application/json")]
[Tags("Locations")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class LocationsController : ControllerBase
{
    private readonly ILocationRepository _locations;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    public LocationsController(
        ILocationRepository locations,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _locations = locations;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    // ============================================================
    // GET: /api/admin/locations
    // ============================================================

    /// <summary>
    /// Gets all locations belonging to the active tenant. Also readable with classes.read/families.read:
    /// the Class and Family forms load this list for their Location dropdowns.
    /// </summary>
    [HttpGet]
    [RequireAnyPermission(Permissions.LocationsRead, Permissions.ClassesRead, Permissions.FamiliesRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<LocationSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? search = null, [FromQuery] bool? active = null, CancellationToken cancellationToken = default)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var locations = await _locations.ListAsync(cancellationToken);

        IEnumerable<Location> result = locations;

        // Search by Location Name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            result = result.Where(location => location.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // Optional Active filter
        if (active.HasValue)
        {
            result = result.Where(location => location.Active == active.Value);
        }

        var page = result.ToList();

        var nameOf = await AuditNamesAsync(page, cancellationToken);

        var summaries = page.Select(location => ToSummary(location, nameOf)).ToList();

        return Ok(ApiResponseFactory.Success(summaries, "Locations retrieved."));
    }

    // ============================================================
    // GET: /api/admin/locations/{id}
    // ============================================================

    /// <summary>
    /// Gets a single location belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.LocationsRead)]
    [ProducesResponseType<ApiResponse<LocationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var location = await _locations.GetByIdAsync(id, cancellationToken);

        if (location is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Location not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(location, cancellationToken), "Location retrieved."));
    }

    // ============================================================
    // POST: /api/admin/locations
    // ============================================================

    /// <summary>
    /// Creates a location for the active tenant.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.LocationsWrite)]
    [ProducesResponseType<ApiResponse<LocationResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Location name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Location name cannot exceed 100 characters."));
        }

        if (await _locations.NameExistsAsync(name, User.GetActiveTenantId(), cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Location name already exists.", name));
        }

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = request.Active

            // TenantId intentionally NOT assigned here.
            // SakoleeDbContext.StampTenant() assigns it
            // from ITenantContext.TenantId.
        };

        await _locations.AddAsync(location, cancellationToken);

        await _audit.AddAsync(nameof(Location), location.Id.ToString(), "Created", details: $"name={location.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync( cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync( location, cancellationToken), "Location created."));
    }

    // ============================================================
    // PUT: /api/admin/locations/{id}
    // ============================================================

    /// <summary>
    /// Updates a location belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.LocationsWrite)]
    [ProducesResponseType<ApiResponse<LocationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var location = await _locations.GetByIdAsync(id, cancellationToken);

        if (location is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Location not found."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.",
                    "Location name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.",
                    "Location name cannot exceed 100 characters."));
        }

        if (await _locations.NameExistsAsync(name, location.TenantId, location.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Location name already exists.", name));
        }

        location.Name = name;
        location.Active = request.Active;

        _locations.Update(location);

        await _audit.AddAsync(nameof(Location), location.Id.ToString(), "Updated",
            details: $"name={location.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(location, cancellationToken),
                "Location updated."));
    }

    // ============================================================
    // DELETE: /api/admin/locations/{id}
    // ============================================================

    /// <summary>
    /// Soft-deletes a location belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.LocationsDelete)]
    [ProducesResponseType<ApiResponse<object>>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var location = await _locations.GetByIdAsync(id, cancellationToken);

        if (location is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Location not found."));
        }

        _locations.Remove(location);

        await _audit.AddAsync(nameof(Location), location.Id.ToString(), "Deleted",
            details: $"name={location.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = location.Id },"Location deleted."));
    }

    // ============================================================
    // Helpers
    // ============================================================

    private bool HasActiveTenant()
        => User.GetActiveTenantId() is not null;

    private IActionResult NoActiveTenant()
        => StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));

    /// <summary>
    /// Resolves user IDs used by audit fields into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Location> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(location => new[]
                {
                    location.CreatedById,
                    location.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<LocationResponse> ToResponseAsync(Location location, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { location }, cancellationToken);

        return new LocationResponse(location.Id, location.TenantId, location.Name, location.Active,
            nameOf(location.CreatedById), location.CreatedOnUtc, nameOf(location.UpdatedById),
            location.UpdatedOnUtc);
    }

    private static LocationSummary ToSummary(Location location, Func<Guid?, string?> nameOf)
        => new(location.Id, location.TenantId, location.Name, location.Active, nameOf(location.CreatedById), location.CreatedOnUtc, nameOf(location.UpdatedById), location.UpdatedOnUtc);
}