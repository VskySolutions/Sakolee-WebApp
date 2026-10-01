using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.ClassRooms;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Class Room management for the active tenant and specific locations.
/// Class rooms belong to the Dance Studio/Tenant and are associated with a location.
/// </summary>
[ApiController]
[Route("/api/admin/class-rooms")]
[Produces("application/json")]
[Tags("Class Rooms")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class ClassRoomsController : ControllerBase
{
    private readonly IClassRoomRepository _classRoomRepository;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassRoomsController"/> class.
    /// </summary>
    public ClassRoomsController(
        IClassRoomRepository classRoomRepository,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _classRoomRepository = classRoomRepository;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #region List Class Rooms

    /// <summary>
    /// Gets all class rooms belonging to the active tenant, with optional location filter.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.ClassRoomsRead)] // Ensure this permission exists in your Permissions class
    [ProducesResponseType<ApiResponse<IReadOnlyList<ClassRoomSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] bool? showDeleted = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var tenantId = User.GetActiveTenantId();

        var (items, total) = await _classRoomRepository.ListAsync(
            search, tenantId, locationId, showDeleted,new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(classRoom => ToSummary(classRoom, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Class rooms retrieved.", page, limit, total));
    }

    #endregion

    #region Get Class Room By ID

    /// <summary>
    /// Gets a single class room belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ClassRoomsRead)]
    [ProducesResponseType<ApiResponse<ClassRoomResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var classRoom = await _classRoomRepository.GetByIdAsync(id, cancellationToken);

        if (classRoom is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class room not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(classRoom, cancellationToken), "Class room retrieved."));
    }

    #endregion

    #region Create Class Room

    /// <summary>
    /// Creates a class room for the active tenant and specified location.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.ClassRoomsWrite)]
    [ProducesResponseType<ApiResponse<ClassRoomResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClassRoomRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        if (request.LocationId == Guid.Empty)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Location ID is required."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Class room name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Class room name cannot exceed 100 characters."));
        }

        var tenantId = User.GetActiveTenantId()!.Value;

        if (await _classRoomRepository.ClassRoomNameExistsAsync(tenantId, request.LocationId, name, null, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Class room with this name already exists in this tenant."));
        }

        var classRoom = new ClassRooms
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationId = request.LocationId,
            Name = name,
            Active = request.Active
        };

        await _classRoomRepository.AddAsync(classRoom, cancellationToken);

        await _audit.AddAsync(nameof(ClassRooms), classRoom.Id.ToString(), "Created", details: $"name={classRoom.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(classRoom, cancellationToken), "Class room created."));
    }

    #endregion

    #region Update Class Room

    /// <summary>
    /// Updates a class room belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ClassRoomsWrite)]
    [ProducesResponseType<ApiResponse<ClassRoomResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRoomRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var classRoom = await _classRoomRepository.GetByIdAsync(id, cancellationToken);

        if (classRoom is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class room not found."));
        }

        if (request.LocationId == Guid.Empty)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Location ID is required."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Class room name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Class room name cannot exceed 100 characters."));
        }

        if (await _classRoomRepository.ClassRoomNameExistsAsync(
     tenantId: classRoom.TenantId!.Value,
     locationId: classRoom.LocationId,
     name: name,
     excludingClassRoomId: classRoom.Id,
     cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Class room with this name already exists."));
        }
        classRoom.LocationId = request.LocationId;
        classRoom.Name = name;
        classRoom.Active = request.Active;

        _classRoomRepository.Update(classRoom);

        await _audit.AddAsync(nameof(ClassRooms), classRoom.Id.ToString(), "Updated",
            details: $"name={classRoom.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(classRoom, cancellationToken), "Class room updated."));
    }

    #endregion

    #region Delete Class Room

    /// <summary>
    /// Soft-deletes a class room belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ClassRoomsDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var classRoom = await _classRoomRepository.GetByIdAsync(id, cancellationToken);

        if (classRoom is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class room not found."));
        }

        _classRoomRepository.Remove(classRoom);

        await _audit.AddAsync(nameof(ClassRooms), classRoom.Id.ToString(), "Deleted",
            details: $"name={classRoom.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = classRoom.Id }, "Class room deleted."));
    }

    #endregion

    #region Helpers

    private bool HasActiveTenant()
        => User.GetActiveTenantId() is not null;

    private IActionResult NoActiveTenant()
        => StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));

    /// <summary>
    /// Resolves user IDs used by audit fields into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<ClassRooms> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(classRoom => new[]
                {
                    classRoom.CreatedById,
                    classRoom.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<ClassRoomResponse> ToResponseAsync(ClassRooms classRoom, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { classRoom }, cancellationToken);

        return new ClassRoomResponse(
            classRoom.Id,
            classRoom.TenantId,
            classRoom.LocationId,
            classRoom.Name,
            classRoom.Active,
            nameOf(classRoom.CreatedById),
            classRoom.CreatedOnUtc,
            nameOf(classRoom.UpdatedById),
            classRoom.UpdatedOnUtc);
    }

    private static ClassRoomSummary ToSummary(ClassRooms classRoom, Func<Guid?, string?> nameOf)
        => new(
            classRoom.Id,
            classRoom.TenantId,
            classRoom.LocationId,
            classRoom.Name,
            classRoom.Active,
            classRoom.Tenant != null ? classRoom.Tenant.Name : string.Empty,
            nameOf(classRoom.CreatedById),
            classRoom.CreatedOnUtc,
            nameOf(classRoom.UpdatedById),
            classRoom.UpdatedOnUtc);

    #endregion
}