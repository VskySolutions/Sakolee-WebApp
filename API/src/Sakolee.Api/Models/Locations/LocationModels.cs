namespace Sakolee.Api.Models.Locations;

#region Create Location Request
public sealed class CreateLocationRequest
{
    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the location is active.
    /// </summary>
    public bool Active { get; set; } = true;
}
#endregion

#region Update Location Request
public sealed class UpdateLocationRequest
{
    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the location is active.
    /// </summary>
    public bool Active { get; set; }
}
#endregion

#region Location Response
public sealed record LocationResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion

#region Location Summary
public sealed record LocationSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion