namespace Sakolee.Domain.Entities;

public class BillingCycle
{
    #region Properties

    public Guid Id { get; set; }

    /// <summary>
    /// Owning tenant — maps to the physical "TenentId" column.
    /// </summary>
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Deleted { get; set; }

    /// <summary>
    /// Navigation property for the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}