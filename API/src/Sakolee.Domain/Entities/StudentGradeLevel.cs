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

    // Navigation Properties
    public Tenant? Tenant { get; set; }
}