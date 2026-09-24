namespace Sakolee.Domain.Entities;

public class BillingMethod : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public Tenant? Tenant { get; set; }
}