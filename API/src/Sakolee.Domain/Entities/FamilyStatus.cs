namespace Sakolee.Domain.Entities;

/// <summary>
/// Tenant-scoped Family Status master record (e.g. "Active", "Prospect", "Withdrawn").
/// Audit fields and soft-delete (Deleted / DeletedOnUtc) come from <see cref="AuditableEntity"/> and are
/// stamped by the DbContext; <see cref="Active"/> is the separate, user-controlled enabled/disabled flag.
/// </summary>
public class FamilyStatus : AuditableEntity
{
    public Guid FamilyStatusId { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public virtual Tenant? Tenant { get; set; }
}
