using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.MembershipTypes;
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
/// Membership Type Master management for the active tenant.
/// Membership types belong to the Dance Studio/Tenant that owns them.
/// </summary>
[ApiController]
[Route("/api/admin/membership-types")]
[Produces("application/json")]
[Tags("Membership Types")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class MembershipTypesController : ControllerBase
{
    private readonly IMembershipTypeRepository _membershipTypeRepository;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipTypesController"/> class.
    /// </summary>
    public MembershipTypesController(
        IMembershipTypeRepository membershipTypeRepository,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _membershipTypeRepository = membershipTypeRepository;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #region List Membership Types

    /// <summary>
    /// Gets all membership types belonging to the active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.MembershipTypesRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<MembershipTypeSummary>>>(StatusCodes.Status200OK)]
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
        var (items, total) = await _membershipTypeRepository.ListAsync(
            search, tenantId, active, showDeleted, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(membershipType => ToSummary(membershipType, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Membership types retrieved.", page, limit, total));
    }

    #endregion

    #region Get Membership Type By ID

    /// <summary>
    /// Gets a single membership type belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.MembershipTypesRead)]
    [ProducesResponseType<ApiResponse<MembershipTypeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var membershipType = await _membershipTypeRepository.GetByIdAsync(id, cancellationToken);

        if (membershipType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Membership type not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(membershipType, cancellationToken), "Membership type retrieved."));
    }

    #endregion

    #region Create Membership Type

    /// <summary>
    /// Creates a membership type for the active tenant.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.MembershipTypesWrite)]
    [ProducesResponseType<ApiResponse<MembershipTypeResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMembershipTypeRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Membership type name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Membership type name cannot exceed 100 characters."));
        }

        if (await _membershipTypeRepository.NameExistsAsync(name, User.GetActiveTenantId(), cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Membership type with this name already exists."));
        }

        var membershipType = new MembershipType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = request.Active

            // TenantId intentionally NOT assigned here.
            // SakoleeDbContext.StampTenant() assigns it
            // from ITenantContext.TenantId.
        };

        await _membershipTypeRepository.AddAsync(membershipType, cancellationToken);

        await _audit.AddAsync(nameof(MembershipType), membershipType.Id.ToString(), "Created", details: $"name={membershipType.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(membershipType, cancellationToken), "Membership type created."));
    }

    #endregion

    #region Update Membership Type

    /// <summary>
    /// Updates a membership type belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.MembershipTypesWrite)]
    [ProducesResponseType<ApiResponse<MembershipTypeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMembershipTypeRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var membershipType = await _membershipTypeRepository.GetByIdAsync(id, cancellationToken);

        if (membershipType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Membership type not found."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Membership type name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Membership type name cannot exceed 100 characters."));
        }

        if (await _membershipTypeRepository.NameExistsAsync(name, membershipType.TenantId, membershipType.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Membership type with this name already exists."));
        }

        membershipType.Name = name;
        membershipType.Active = request.Active;

        _membershipTypeRepository.Update(membershipType);

        await _audit.AddAsync(nameof(MembershipType), membershipType.Id.ToString(), "Updated",
            details: $"name={membershipType.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(membershipType, cancellationToken), "Membership type updated."));
    }

    #endregion

    #region Delete Membership Type

    /// <summary>
    /// Soft-deletes a membership type belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.MembershipTypesDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var membershipType = await _membershipTypeRepository.GetByIdAsync(id, cancellationToken);

        if (membershipType is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Membership type not found."));
        }

        _membershipTypeRepository.Remove(membershipType);

        await _audit.AddAsync(nameof(MembershipType), membershipType.Id.ToString(), "Deleted",
            details: $"name={membershipType.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = membershipType.Id }, "Membership type deleted."));
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
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<MembershipType> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(membershipType => new[]
                {
                    membershipType.CreatedById,
                    membershipType.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<MembershipTypeResponse> ToResponseAsync(MembershipType membershipType, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { membershipType }, cancellationToken);

        return new MembershipTypeResponse(
            membershipType.Id,
            membershipType.TenantId,
            membershipType.Name,
            membershipType.Active,
            nameOf(membershipType.CreatedById),
            membershipType.CreatedOnUtc,
            nameOf(membershipType.UpdatedById),
            membershipType.UpdatedOnUtc);
    }

    private static MembershipTypeSummary ToSummary(MembershipType membershipType, Func<Guid?, string?> nameOf)
        => new(
            membershipType.Id,
            membershipType.TenantId,
            membershipType.Name,
            membershipType.Active,
            nameOf(membershipType.CreatedById),
            membershipType.CreatedOnUtc,
            nameOf(membershipType.UpdatedById),
            membershipType.UpdatedOnUtc);

    #endregion
}