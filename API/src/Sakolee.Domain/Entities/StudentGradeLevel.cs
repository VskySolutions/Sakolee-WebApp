using Sakolee.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StudentGradeLevel : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    /// <summary>Stable key of a seeded default row (null for rows a tenant adds itself).</summary>
    public string? Code { get; set; }

    /// <summary>Template rows (in the template tenant) with this set are copied into every new tenant.</summary>
    public bool IsSystem { get; set; }

    // Navigation Properties
    public Tenant? Tenant { get; set; }
}