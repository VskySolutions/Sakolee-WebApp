namespace Sakolee.Api.Models.TShirtSizes;

/// <summary>
/// Represents the T-Shirt Size data returned by the API.
/// </summary>
public sealed record TShirtSizeSummary(Guid TShirtSizeId,string Name,Guid TenantId,string TenantName, string? CreatedBy,DateTime CreatedOnUtc,string? UpdatedBy,DateTime? UpdatedOnUtc);

/// <summary>
/// Request used to create a new T-Shirt Size.
/// </summary>
public sealed record CreateTShirtSizeRequest(string Name);

/// <summary>
/// Request used to update an existing T-Shirt Size.
/// </summary>
public sealed record UpdateTShirtSizeRequest(string Name);