namespace Sakolee.Domain.Entities;

/// <summary>
/// Assigns a <see cref="Person"/> to a <see cref="Entities.Tenant"/> that owns (part of) their CRM
/// record. Kept as its own table rather than a <c>TenantId</c> column on <see cref="Person"/> itself: the
/// assignment carries its own provenance (who set it, when) the way <see cref="UserTenantRole"/> does
/// for a user's tenant/role grants, and a person having no row here is exactly a platform-level person
/// with no owning tenant.
/// <para>
/// A person may hold several of these — one per tenant they belong to — enforced only against duplicates
/// (one active row per (PersonId, TenantId) pair; see the unique filtered index in
/// <c>TenantPersonMappingConfiguration</c>).
/// </para>
/// </summary>
public class TenantPersonMapping : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Tenant? Tenant { get; set; }

    public Person? Person { get; set; }
}
