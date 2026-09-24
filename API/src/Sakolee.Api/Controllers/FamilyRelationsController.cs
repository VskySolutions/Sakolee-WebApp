using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.FamilyRelations;
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
/// Family Relation Master management for the active tenant.
/// Family relations belong to the Dance Studio/Tenant that owns them.
/// </summary>
[ApiController]
[Route("/api/admin/family-relations")]
[Produces("application/json")]
[Tags("Family Relations")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class FamilyRelationsController : ControllerBase
{
    private readonly IFamilyRelationRepository _familyRelationRepository;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyRelationsController"/> class.
    /// </summary>
    public FamilyRelationsController(
        IFamilyRelationRepository familyRelationRepository,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _familyRelationRepository = familyRelationRepository;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #region List Family Relations

    /// <summary>
    /// Gets all family relations belonging to the active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.FamilyRelationsRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<FamilyRelationSummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? active = null,
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

        var (items, total) = await _familyRelationRepository.ListAsync(
            search, tenantId, active, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(familyRelation => ToSummary(familyRelation, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Family relations retrieved.", page, limit, total));
    }

    #endregion

    #region Get Family Relation By ID

    /// <summary>
    /// Gets a single family relation belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.FamilyRelationsRead)]
    [ProducesResponseType<ApiResponse<FamilyRelationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var familyRelation = await _familyRelationRepository.GetByIdAsync(id, cancellationToken);

        if (familyRelation is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family relation not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(familyRelation, cancellationToken), "Family relation retrieved."));
    }

    #endregion

    #region Create Family Relation

    /// <summary>
    /// Creates a family relation for the active tenant.
    /// TenantId is assigned automatically by the persistence layer.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.FamilyRelationsWrite)]
    [ProducesResponseType<ApiResponse<FamilyRelationResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFamilyRelationRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Family relation name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Family relation name cannot exceed 100 characters."));
        }

        if (await _familyRelationRepository.NameExistsAsync(name, User.GetActiveTenantId(), cancellationToken: cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Family relation with this name already exists."));
        }

        var familyRelation = new FamilyRelation
        {
            Id = Guid.NewGuid(),
            Name = name,
            Active = request.Active,
            TenantId = User.GetActiveTenantId()!.Value,

            
        };

        await _familyRelationRepository.AddAsync(familyRelation, cancellationToken);

        await _audit.AddAsync(nameof(FamilyRelation), familyRelation.Id.ToString(), "Created", details: $"name={familyRelation.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(familyRelation, cancellationToken), "Family relation created."));
    }

    #endregion

    #region Update Family Relation

    /// <summary>
    /// Updates a family relation belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.FamilyRelationsWrite)]
    [ProducesResponseType<ApiResponse<FamilyRelationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyRelationRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var familyRelation = await _familyRelationRepository.GetByIdAsync(id, cancellationToken);

        if (familyRelation is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family relation not found."));
        }

        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Family relation name is required."));
        }

        if (name.Length > 100)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Family relation name cannot exceed 100 characters."));
        }

        if (await _familyRelationRepository.NameExistsAsync(name, familyRelation.TenantId, familyRelation.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A Family relation with this name already exists."));
        }

        familyRelation.Name = name;
        familyRelation.Active = request.Active;

        _familyRelationRepository.Update(familyRelation);

        await _audit.AddAsync(nameof(FamilyRelation), familyRelation.Id.ToString(), "Updated",
            details: $"name={familyRelation.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(familyRelation, cancellationToken), "Family relation updated."));
    }

    #endregion

    #region Delete Family Relation

    /// <summary>
    /// Soft-deletes a family relation belonging to the active tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.FamilyRelationsDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant(); // or NoActiveTenant
        }

        var familyRelation = await _familyRelationRepository.GetByIdAsync(id, cancellationToken);

        if (familyRelation is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Family relation not found."));
        }

        _familyRelationRepository.Remove(familyRelation);

        await _audit.AddAsync(nameof(FamilyRelation), familyRelation.Id.ToString(), "Deleted",
            details: $"name={familyRelation.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = familyRelation.Id }, "Family relation deleted."));
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
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<FamilyRelation> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(familyRelation => new[]
                {
                    familyRelation.CreatedById,
                    familyRelation.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    private async Task<FamilyRelationResponse> ToResponseAsync(FamilyRelation familyRelation, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { familyRelation }, cancellationToken);

        return new FamilyRelationResponse(
            familyRelation.Id,
            familyRelation.TenantId,
            familyRelation.Name,
            familyRelation.Active,
            nameOf(familyRelation.CreatedById),
            familyRelation.CreatedOnUtc,
           nameOf(familyRelation.UpdatedById),
            familyRelation.UpdatedOnUtc);
    }

    private static FamilyRelationSummary ToSummary(FamilyRelation familyRelation, Func<Guid?, string?> nameOf)
        => new(
            familyRelation.Id,
            familyRelation.TenantId,
            familyRelation.Name,
            
            familyRelation.Active,
            familyRelation.Tenant != null ? familyRelation.Tenant.Name : null,
            nameOf(familyRelation.CreatedById),
            familyRelation.CreatedOnUtc,
            nameOf(familyRelation.UpdatedById),
            familyRelation.UpdatedOnUtc);

    #endregion
}