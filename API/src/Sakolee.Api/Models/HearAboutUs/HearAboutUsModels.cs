namespace Sakolee.Api.Models.HearAboutUs;

/// <summary>
/// Represents the Hear About Us data returned by the API.
/// </summary>
public sealed record HearAboutUsSummary(Guid HearAboutUsId,string Name, bool Active, bool Deleted, Guid TenantId, string TenantName, string? CreatedBy, DateTime CreatedOnUtc, string? UpdatedBy, DateTime UpdatedOnUtc);

/// <summary>
/// Request used to create a new Hear About Us record.
/// </summary>
public sealed record CreateHearAboutUsRequest(string Name, bool Active = true);

/// <summary>
/// Request used to update an existing Hear About Us record.
/// </summary>
public sealed record UpdateHearAboutUsRequest(string Name, bool Active);