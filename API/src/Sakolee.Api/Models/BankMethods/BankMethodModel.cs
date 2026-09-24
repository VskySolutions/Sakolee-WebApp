namespace Sakolee.Api.Models.BankMethods;

#region Create Bank Method Request
public sealed class CreateBankMethodRequest
{
    /// <summary>
    /// Gets or sets the name of the bank method.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the bank method is active.
    /// </summary>
    public bool Active { get; set; } = true;
}
#endregion

#region Update Bank Method Request
public sealed class UpdateBankMethodRequest
{
    /// <summary>
    /// Gets or sets the name of the bank method.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the bank method is active.
    /// </summary>
    public bool Active { get; set; }
}
#endregion

#region Bank Method Response
public sealed record BankMethodResponse(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion

#region Bank Method Summary
public sealed record BankMethodSummary(
    Guid Id,
    Guid? TenantId,
    string Name,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime UpdatedOnUtc);
#endregion