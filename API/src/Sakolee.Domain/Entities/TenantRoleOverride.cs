namespace Sakolee.Domain.Entities;

/// <summary>
/// One tenant's own permission set for a PLATFORM <see cref="Role"/>. A platform role is shared by every
/// tenant and only a Super Admin may change it; a tenant admin who edits it instead writes one of these,
/// which replaces the role's direct <see cref="Role.Permissions"/> inside that tenant only. The role's
/// identity (Id, Name) is untouched, so assignments, name lookups and role claims keep working; removing
/// the row returns the tenant to the platform default.
/// </summary>
public class TenantRoleOverride : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RoleId { get; set; }

    /// <summary>The tenant's direct permission keys for the role. Mapped as a JSON column.</summary>
    public List<string> Permissions { get; set; } = new();

    public Role? Role { get; set; }
}
