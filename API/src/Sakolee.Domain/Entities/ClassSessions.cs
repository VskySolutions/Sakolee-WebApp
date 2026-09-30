namespace Sakolee.Domain.Entities;

/// <summary>
/// Tenant-scoped Class Session master record (offered in the Class form's Session dropdown).
/// Audit fields and soft-delete come from <see cref="AuditableEntity"/> (stamped by the DbContext);
/// <see cref="Active"/> is the separate, user-controlled enabled/disabled flag.
/// </summary>
public class ClassSessions : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public virtual Tenant? Tenant { get; set; }
}
