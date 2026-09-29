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
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}