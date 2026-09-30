namespace Sakolee.Api.Models.MembershipTypes;

#region Create Membership Type Request
public sealed class CreateMembershipTypeRequest
{
    /// <summary>
    /// Gets or sets the name of the membership type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the membership type is active.
    /// </summary>
    public bool Active { get; set; } = true;
}
#endregion

#region Update Membership Type Request
public sealed class UpdateMembershipTypeRequest
{
    /// <summary>
    /// Gets or sets the name of the membership type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the membership type is active.
    /// </summary>
    public bool Active { get; set; }
}
#endregion

#region Membership Type Response
public sealed record MembershipTypeResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion

#region Membership Type Summary
public sealed record MembershipTypeSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion