namespace Sakolee.Api.Models.AccountType;

/// <summary>
/// Represents the Account Type data returned by the API.
/// </summary>
public sealed record AccountTypeSummary(Guid AccountTypeId,string Name,bool Active,bool Deleted,Guid TenantId,string TenantName,string? CreatedBy,DateTime CreatedOnUtc,string? UpdatedBy,DateTime? UpdatedOnUtc);

/// <summary>
/// Request used to create a new Account Type record.
/// </summary>
public sealed record CreateAccountTypeRequest(string Name,bool Active = true);

/// <summary>
/// Request used to update an existing Account Type record.
/// </summary>
public sealed record UpdateAccountTypeRequest(string Name,bool Active);