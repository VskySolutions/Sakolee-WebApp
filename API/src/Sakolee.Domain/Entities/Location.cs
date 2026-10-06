

namespace Sakolee.Domain.Entities;

public class Location : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    /// <summary>Stable key of a seeded default row (null for rows a tenant adds itself).</summary>
    public string? Code { get; set; }

    /// <summary>Template rows (in the template tenant) with this set are copied into every new tenant.</summary>
    public bool IsSystem { get; set; }

    public Tenant? Tenant { get; set; }
}

