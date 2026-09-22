namespace Sakolee.Api.Models.Locations;

public sealed class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}

public sealed class UpdateLocationRequest
{
    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; }
}

public sealed record LocationResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);

public sealed record LocationSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);