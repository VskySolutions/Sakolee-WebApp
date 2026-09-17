using Sakolee.Domain.Enums;

namespace Sakolee.Domain.Entities;

/// <summary>
/// An independent organization served by the platform. Owns its own isolated data
/// and users (Multi-Tenancy).
/// </summary>
public class Tenant : AuditableEntity
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable tenant name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique, URL-safe slug. Immutable after creation.</summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>IANA time zone id used to render this tenant's UTC timestamps. Defaults to UTC.</summary>
    public string TimeZoneId { get; set; } = "UTC";

    /// <summary>Lifecycle status. Defaults to Active on creation (AC-TNT-001.4).</summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>UTC timestamp when the tenant was created.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>The tenant's own business address (optional; reusable <see cref="Entities.Address"/> record).</summary>
    public Guid? AddressId { get; set; }

    public Address? Address { get; set; }

    /// <summary>
    /// The tenant's default administrator — the <see cref="Entities.Person"/> master record created
    /// alongside the tenant, whose linked <see cref="Person.UserId"/> account holds the TenantAdmin role
    /// and is protected from deactivation/role removal (see <see cref="User.IsProtected"/>).
    /// </summary>
    public Guid? PersonId { get; set; }

    public Person? Person { get; set; }
}
