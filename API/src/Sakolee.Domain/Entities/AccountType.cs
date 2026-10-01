namespace Sakolee.Domain.Entities;

/// <summary>
/// Represents an Account Type master record.
/// Each record belongs to a specific tenant.
/// </summary>
public class AccountType : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the unique identifier of the Account Type record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// Maps to the physical "TenentId" column.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name of the Account Type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the Account Type is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}