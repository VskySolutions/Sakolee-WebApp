namespace Sakolee.Api.Models.EPaymentSchedule;

/// <summary>
/// Represents the E-Payment Schedule data returned by the API.
/// </summary>
public sealed record EPaymentScheduleSummary(Guid EPaymentScheduleId,string Name, bool Active, bool Deleted, Guid TenantId, string TenantName,string? CreatedBy, DateTime CreatedOnUtc, string? UpdatedBy, DateTime UpdatedOnUtc);

/// <summary>
/// Request used to create a new E-Payment Schedule record.
/// </summary>
public sealed record CreateEPaymentScheduleRequest(string Name,bool Active = true);

/// <summary>
/// Request used to update an existing E-Payment Schedule record.
/// </summary>
public sealed record UpdateEPaymentScheduleRequest(string Name,bool Active);