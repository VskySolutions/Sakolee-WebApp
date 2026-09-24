namespace Sakolee.Api.Models.BillingCycles;

/// <summary>
/// Represents the Billing Cycle data returned by the API.
/// </summary>
public sealed record BillingCycleSummary(Guid BillingCycleId, string Name, Guid TenantId, string TenantName, string? CreatedBy,DateTime CreatedOnUtc,string? UpdatedBy,DateTime? UpdatedOnUtc);

/// <summary>
/// Request used to create a new Billing Cycle.
/// </summary>
public sealed record CreateBillingCycleRequest(string Name);

/// <summary>
/// Request used to update an existing Billing Cycle.
/// </summary>
public sealed record UpdateBillingCycleRequest(string Name);