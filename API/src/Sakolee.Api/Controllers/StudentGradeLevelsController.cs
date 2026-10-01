using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.StudentGradeLevels;
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
/// Student Grade Level Master management for the active tenant.
/// Student grade levels belong to the Dance Studio/Tenant that owns them.
/// </summary>
[ApiController]
[Route("/api/admin/student-grade-levels")]
[Produces("application/json")]
[Tags("Student Grade Levels")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class StudentGradeLevelsController : ControllerBase
{
    private readonly IStudentGradeLevelRepository _studentGradeLevelRepository;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentGradeLevelsController"/> class.
    /// </summary>
    public StudentGradeLevelsController(
        IStudentGradeLevelRepository studentGradeLevelRepository,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _studentGradeLevelRepository = studentGradeLevelRepository;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #region List Student Grade Levels

    /// <summary>
    /// Gets all student grade levels belonging to the active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.StudentGradeLevelsRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<StudentGradeLevelSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? active = null,
        [FromQuery] bool? showDeleted = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        // Ensure valid pagination boundary values
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var tenantId = User.GetActiveTenantId();

        // Fetch paginated, filtered, and sorted records from repository
        var (items, total) = await _studentGradeLevelRepository.ListAsync(
            search, tenantId, active, showDeleted, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(studentGradeLevel => ToSummary(studentGradeLevel, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Student grade levels retrieved.", page, limit, total));
    }

    #endregion

    #region Get Student Grade Level By ID

    /// <summary>
    /// Gets a single student grade level belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.StudentGradeLevelsRead)]
    [ProducesResponseType<ApiResponse<StudentGradeLevelResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var studentGradeLevel = await _studentGradeLevelRepository.GetByIdAsync(id, cancellationToken);

        if (studentGradeLevel is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student grade level not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(studentGradeLevel, cancellationToken), "Student grade level retrieved."));
    }

    #endregion

    #region Create Student Grade Level

    /// <summary>
    /// Creates a student grade level for the active tenant.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.StudentGradeLevelsWrite)]
    [ProducesResponseType<ApiResponse<StudentGradeLevelResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStudentGradeLevelRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Student grade level name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Student grade level name cannot exceed 100 characters."));
        }

        if (await _studentGradeLevelRepository.NameExistsAsync(name, User.GetActiveTenantId(), cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Student grade level with this name already exists."));
        }

        var studentGradeLevel = new StudentGradeLevel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = request.Active

            // TenantId intentionally NOT assigned here.
            // SakoleeDbContext.StampTenant() assigns it
            // from ITenantContext.TenantId.
        };

        await _studentGradeLevelRepository.AddAsync(studentGradeLevel, cancellationToken);

        await _audit.AddAsync(nameof(StudentGradeLevel), studentGradeLevel.Id.ToString(), "Created", details: $"name={studentGradeLevel.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(studentGradeLevel, cancellationToken), "Student grade level created."));
    }

    #endregion

    #region Update Student Grade Level

    /// <summary>
    /// Updates a student grade level belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.StudentGradeLevelsWrite)]
    [ProducesResponseType<ApiResponse<StudentGradeLevelResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentGradeLevelRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var studentGradeLevel = await _studentGradeLevelRepository.GetByIdAsync(id, cancellationToken);

        if (studentGradeLevel is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student grade level not found."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Student grade level name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Student grade level name cannot exceed 100 characters."));
        }

        if (await _studentGradeLevelRepository.NameExistsAsync(name, studentGradeLevel.TenantId, studentGradeLevel.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Student grade level with this name already exists."));
        }

        studentGradeLevel.Name = name;
        studentGradeLevel.Active = request.Active;

        _studentGradeLevelRepository.Update(studentGradeLevel);

        await _audit.AddAsync(nameof(StudentGradeLevel), studentGradeLevel.Id.ToString(), "Updated",
            details: $"name={studentGradeLevel.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(studentGradeLevel, cancellationToken), "Student grade level updated."));
    }

    #endregion

    #region Delete Student Grade Level

    /// <summary>
    /// Soft-deletes a student grade level belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.StudentGradeLevelsDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var studentGradeLevel = await _studentGradeLevelRepository.GetByIdAsync(id, cancellationToken);

        if (studentGradeLevel is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student grade level not found."));
        }

        _studentGradeLevelRepository.Remove(studentGradeLevel);

        await _audit.AddAsync(nameof(StudentGradeLevel), studentGradeLevel.Id.ToString(), "Deleted",
            details: $"name={studentGradeLevel.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = studentGradeLevel.Id }, "Student grade level deleted."));
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
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<StudentGradeLevel> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(studentGradeLevel => new[]
                {
                    studentGradeLevel.CreatedById,
                    studentGradeLevel.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<StudentGradeLevelResponse> ToResponseAsync(StudentGradeLevel studentGradeLevel, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { studentGradeLevel }, cancellationToken);

        return new StudentGradeLevelResponse(
            studentGradeLevel.Id,
            studentGradeLevel.TenantId,
            studentGradeLevel.Name,
            studentGradeLevel.Active,
            nameOf(studentGradeLevel.CreatedById),
            studentGradeLevel.CreatedOnUtc,
            nameOf(studentGradeLevel.UpdatedById),
            studentGradeLevel.UpdatedOnUtc);
    }

    private static StudentGradeLevelSummary ToSummary(StudentGradeLevel studentGradeLevel, Func<Guid?, string?> nameOf)
        => new(
            studentGradeLevel.Id,
            studentGradeLevel.TenantId,
            studentGradeLevel.Name,
            studentGradeLevel.Active,
            nameOf(studentGradeLevel.CreatedById),
            studentGradeLevel.CreatedOnUtc,
            nameOf(studentGradeLevel.UpdatedById),
            studentGradeLevel.UpdatedOnUtc);

    #endregion
}