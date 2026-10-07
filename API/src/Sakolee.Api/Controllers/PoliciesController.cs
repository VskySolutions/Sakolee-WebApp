using Microsoft.AspNetCore.Mvc;
using Sakolee.Api.Models;
using Sakolee.Api.Models.Policies;
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
/// Policy management for the active tenant. A policy is a named block of rich-text content that can be
/// attached to one or more classes.
/// </summary>
[ApiController]
[Route("/api/admin/policies")]
[Produces("application/json")]
[Tags("Policies")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class PoliciesController : ControllerBase
{
    #region Fields

    /// <summary>Matches the <c>Policy.Name</c> column (nvarchar(200)).</summary>
    private const int NameMaxLength = 200;

    /// <summary>Matches the <c>Policy.Description</c> column (nvarchar(1000)).</summary>
    private const int DescriptionMaxLength = 1000;

    private readonly IPolicyRepository _policyRepository;
    private readonly IUserRepository _users;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PoliciesController"/> class.
    /// </summary>
    public PoliciesController(
        IPolicyRepository policyRepository,
        IUserRepository users,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _users = users;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region List Policies

    /// <summary>
    /// Gets all policies belonging to the active tenant.
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.PoliciesRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<PolicySummary>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? showDeleted = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        // Keep paging inside sane bounds (page 1+, at most 100 rows).
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var tenantId = User.GetActiveTenantId();

        // Load one page of the tenant's policies.
        var (items, total) = await _policyRepository.ListAsync(
            search, tenantId, showDeleted, new SortRequest(sortBy, descending), page, limit,
            cancellationToken: cancellationToken);

        // Resolve Created By / Updated By ids to display names in one lookup.
        var nameOf = await AuditNamesAsync(items, cancellationToken);

        var summaries = items.Select(policy => ToSummary(policy, nameOf)).ToList();

        return Ok(ApiResponseFactory.Paginated(summaries, "Policies retrieved.", page, limit, total));
    }

    #endregion

    #region Get Policy By ID

    /// <summary>
    /// Gets a single policy (with its content and classes) belonging to the active tenant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.PoliciesRead)]
    [ProducesResponseType<ApiResponse<PolicyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var policy = await _policyRepository.GetByIdAsync(id, cancellationToken);

        if (policy is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Policy not found."));
        }

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(policy, cancellationToken), "Policy retrieved."));
    }

    #endregion

    #region Create Policy

    /// <summary>
    /// Creates a policy for the active tenant.
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.PoliciesWrite)]
    [ProducesResponseType<ApiResponse<PolicyResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        // Normalize and validate the input.
        var name = request.Name?.Trim() ?? string.Empty;
        var description = NormalizeOptional(request.Description);

        if (Validate(name, description) is { } invalid)
        {
            return invalid;
        }

        var tenantId = User.GetActiveTenantId()!.Value;

        // Policy names are unique within the tenant.
        if (await _policyRepository.PolicyNameExistsAsync(tenantId, name, null, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A policy with this name already exists."));
        }

        // Build the policy; audit fields are stamped by the DbContext on save.
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            Content = NormalizeOptional(request.Content),
            Active = request.Active,
            DisplayOrder = request.DisplayOrder
        };

        // Add the policy with its class mappings, record the audit entry, and save in one transaction.
        await _policyRepository.AddAsync(policy, cancellationToken);
        _policyRepository.SetClasses(policy, request.ClassIds ?? new List<Guid>());

        await _audit.AddAsync(nameof(Policy), policy.Id.ToString(), "Created", details: $"name={policy.Name}", cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponseFactory.Success(await ToResponseAsync(policy, cancellationToken), "Policy created."));
    }

    #endregion

    #region Update Policy

    /// <summary>
    /// Updates a policy belonging to the active tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.PoliciesWrite)]
    [ProducesResponseType<ApiResponse<PolicyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePolicyRequest request, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var policy = await _policyRepository.GetByIdAsync(id, cancellationToken);

        if (policy is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Policy not found."));
        }

        // Normalize and validate the input.
        var name = request.Name?.Trim() ?? string.Empty;
        var description = NormalizeOptional(request.Description);

        if (Validate(name, description) is { } invalid)
        {
            return invalid;
        }

        // Policy names are unique within the tenant (this policy itself excluded).
        if (await _policyRepository.PolicyNameExistsAsync(policy.TenantId, name, policy.Id, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Validation failed.", "A policy with this name already exists."));
        }

        // Apply the changes.
        policy.Name = name;
        policy.Description = description;
        policy.Content = NormalizeOptional(request.Content);
        policy.Active = request.Active;
        policy.DisplayOrder = request.DisplayOrder;

        // Classes are replaced only when sent; the grid's status toggle sends none and keeps them.
        if (request.ClassIds is not null)
        {
            _policyRepository.SetClasses(policy, request.ClassIds);
        }

        _policyRepository.Update(policy);

        await _audit.AddAsync(nameof(Policy), policy.Id.ToString(), "Updated",
            details: $"name={policy.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(await ToResponseAsync(policy, cancellationToken), "Policy updated."));
    }

    #endregion

    #region Delete Policy

    /// <summary>
    /// Soft-deletes a policy belonging to the active tenant and detaches it from its classes.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.PoliciesDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!HasActiveTenant())
        {
            return NoActiveTenant();
        }

        var policy = await _policyRepository.GetByIdAsync(id, cancellationToken);

        if (policy is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Policy not found."));
        }

        // Soft-delete the policy and drop its class mappings.
        _policyRepository.Remove(policy);

        await _audit.AddAsync(nameof(Policy), policy.Id.ToString(), "Deleted",
            details: $"name={policy.Name}",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(new { id = policy.Id }, "Policy deleted."));
    }

    #endregion

    #region Helpers

    /// <summary>True when the caller has an active tenant selected.</summary>
    private bool HasActiveTenant()
        => User.GetActiveTenantId() is not null;

    /// <summary>The 403 response returned when the caller has no active tenant.</summary>
    private IActionResult NoActiveTenant()
        => StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("No active tenant for the caller."));

    /// <summary>
    /// Validates the name and description against the column limits; returns the 400 response, or null when valid.
    /// </summary>
    private IActionResult? Validate(string name, string? description)
    {
        if (name.Length == 0)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", "Policy name is required."));
        }

        if (name.Length > NameMaxLength)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", $"Policy name cannot exceed {NameMaxLength} characters."));
        }

        if (description?.Length > DescriptionMaxLength)
        {
            return BadRequest(ApiResponseFactory.Error(ApiErrorCodes.ValidationFailed, "Validation failed.", $"Description cannot exceed {DescriptionMaxLength} characters."));
        }

        return null;
    }

    /// <summary>Trims an optional text value; blank becomes null.</summary>
    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Resolves user IDs used by audit fields into display names.
    /// </summary>
    private async Task<Func<Guid?, string?>> AuditNamesAsync(IEnumerable<Policy> rows, CancellationToken cancellationToken)
    {
        var ids = rows.SelectMany(policy => new[]
                {
                    policy.CreatedById,
                    policy.UpdatedById
                }).Where(id => id.HasValue).Select(id => id!.Value);

        var names = await _users.GetFullNamesAsync(ids, cancellationToken);

        return id => id is { } userId && names.TryGetValue(userId, out var name) ? name : null;
    }

    /// <summary>Maps a policy to the detail response (with content and class ids).</summary>
    private async Task<PolicyResponse> ToResponseAsync(Policy policy, CancellationToken cancellationToken)
    {
        var nameOf = await AuditNamesAsync(new[] { policy }, cancellationToken);

        return new PolicyResponse(
            policy.Id,
            policy.TenantId,
            policy.Name,
            policy.Description,
            policy.Content,
            policy.Active,
            policy.DisplayOrder,
            policy.ClassMappings.Select(m => m.ClassId).ToList(),
            nameOf(policy.CreatedById),
            policy.CreatedOnUtc,
            nameOf(policy.UpdatedById),
            policy.UpdatedOnUtc);
    }

    /// <summary>Maps a policy to a grid row (no content).</summary>
    private static PolicySummary ToSummary(Policy policy, Func<Guid?, string?> nameOf)
        => new(
            policy.Id,
            policy.TenantId,
            policy.Name,
            policy.Description,
            policy.Active,
            policy.DisplayOrder,
            policy.Deleted,
            policy.ClassMappings.Select(m => m.ClassId).ToList(),
            nameOf(policy.CreatedById),
            policy.CreatedOnUtc,
            nameOf(policy.UpdatedById),
            policy.UpdatedOnUtc);

    #endregion
}
