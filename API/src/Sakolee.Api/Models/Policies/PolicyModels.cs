namespace Sakolee.Api.Models.Policies;

#region Create Policy Request

/// <summary>
/// Represents the payload required to create a new policy record.
/// </summary>
public sealed class CreatePolicyRequest
{
    /// <summary>Policy name; required, unique within the tenant, up to 200 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short plain-text summary, up to 1000 characters.</summary>
    public string? Description { get; set; }

    /// <summary>The policy text as HTML from the rich-text editor.</summary>
    public string? Content { get; set; }

    public bool Active { get; set; } = true;

    /// <summary>Sort position in pickers (lowest first).</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Classes the policy applies to.</summary>
    public List<Guid> ClassIds { get; set; } = new();
}

#endregion

#region Update Policy Request

/// <summary>
/// Represents the payload required to update an existing policy record.
/// </summary>
public sealed class UpdatePolicyRequest
{
    /// <summary>Policy name; required, unique within the tenant, up to 200 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short plain-text summary, up to 1000 characters.</summary>
    public string? Description { get; set; }

    /// <summary>The policy text as HTML from the rich-text editor.</summary>
    public string? Content { get; set; }

    public bool Active { get; set; }

    /// <summary>Sort position in pickers (lowest first).</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Classes the policy applies to; null leaves the current mappings unchanged (e.g. a status toggle).</summary>
    public List<Guid>? ClassIds { get; set; }
}

#endregion

#region Policy Response

/// <summary>
/// Represents the detailed response data contract for a policy entity, including its content.
/// </summary>
public sealed record PolicyResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string? Content,
    bool Active,
    int DisplayOrder,
    IReadOnlyList<Guid> ClassIds,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);

#endregion

#region Policy Summary

/// <summary>
/// Represents a policy row in the Policies grid. Content is left out to keep the list light.
/// </summary>
public sealed record PolicySummary(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool Active,
    int DisplayOrder,
    bool Deleted,
    IReadOnlyList<Guid> ClassIds,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);

#endregion
