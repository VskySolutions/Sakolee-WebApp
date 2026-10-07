namespace Sakolee.Domain.Entities;

public class BillingCycle : AuditableEntity
{
    #region Properties

    public Guid Id { get; set; }

    /// <summary>
    /// Owning tenant — maps to the physical "TenentId" column.
    /// </summary>
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the Billing Cycle is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>Stable key of a seeded default row (null for rows a tenant adds itself).</summary>
    public string? Code { get; set; }

    /// <summary>Template rows (in the template tenant) with this set are copied into every new tenant.</summary>
    public bool IsSystem { get; set; }
    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}