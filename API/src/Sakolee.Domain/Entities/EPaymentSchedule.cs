namespace Sakolee.Domain.Entities;

/// <summary>
/// Represents an E-Payment Schedule master record.
/// Each record belongs to a specific tenant.
/// </summary>
public class EPaymentSchedule : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the unique identifier of the E-Payment Schedule record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// Maps to the physical "TenentId" column.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name of the E-Payment Schedule option.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the E-Payment Schedule option is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}