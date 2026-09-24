namespace Sakolee.Domain.Entities;

/// <summary>
/// A family/household record. Maps onto the physical <c>Families</c> table — originally a legacy table
/// named <c>Parents</c> (like <see cref="Student"/>'s <c>Students</c> and
/// <see cref="Entities.Person"/>'s <c>Persons</c>), managed outside this project's original migration
/// history, which <see cref="Student.FamilyId"/> already had a real foreign key onto before this entity
/// existed. Renamed to <c>Families</c> by <c>RenameParentsAndParentContactsToFamilies</c>; the FK on
/// <see cref="Student.FamilyId"/> (<c>FK_Students_Families</c>, renamed alongside it) still targets the
/// same physical table, just under its new name.
/// <para>
/// Unlike a clean design, this row inlines the PRIMARY contact's identity (<see cref="FirstName"/>,
/// <see cref="LastName"/>, <see cref="Email"/>, phones) directly rather than pointing at a
/// <see cref="Entities.Person"/> exclusively — that is how the physical table already shapes it. This
/// entity ALSO carries <see cref="PersonId"/>, linking the primary contact to a real CRM Person (and,
/// from there, its own login account) the same way <see cref="Student"/> does — see
/// <c>FamiliesController.Create</c>. A second (optional) household contact lives in
/// <see cref="FamilyPersonMapping"/> (<c>FamilyPersonMapping</c> table, renamed from
/// <c>FamilyContacts</c>, itself renamed from <c>ParentContacts</c>), one row per additional contact.
/// </para>
/// <para>
/// <see cref="TenantId"/>, <see cref="StudioLocationId"/>, <see cref="FamilyStatusId"/>,
/// <see cref="Source"/>, <see cref="ReferralName"/>, <see cref="EmergencyContactPerson"/>,
/// <see cref="EmergencyPhone"/>, and <see cref="HealthInsuranceCarrier"/> did not exist on the legacy
/// table — they were added by the <c>ExtendParentsAndParentContactsForFamilies</c> migration to carry
/// the rest of the Families data model, the same way <c>AddStudentColumnsForPrototypeForm</c> extended
/// the legacy <c>Students</c> table.
/// </para>
/// </summary>
public class Family
{
    public Guid Id { get; set; }

    /// <summary>The primary contact's linked CRM Person — minted alongside every family (there is
    /// always a primary contact), the same way a Student's PersonId is minted on create.</summary>
    public Guid? PersonId { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>The studio/branch this family registered at.</summary>
    public Guid? StudioLocationId { get; set; }

    /// <summary>Lookup value from <see cref="Entities.FamilyStatus"/> (e.g. Active, Prospect, Inactive).</summary>
    public Guid? FamilyStatusId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    /// <summary>The household's own label, e.g. "The Miller Family".</summary>
    public string? FamilyName { get; set; }

    // ---- Primary contact identity (inlined on this row — see class remarks) ----
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthDate { get; set; }

    /// <summary>Free-text relationship to the student(s), e.g. "Mother", "Father", "Guardian". Maps to
    /// the physical "Type" column.</summary>
    public string? Type { get; set; }

    public string? Email { get; set; }
    public string? HomePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? CellPhone { get; set; }
    public string? Fax { get; set; }
    public string? OtherPhone { get; set; }

    // ---- Household address (inlined on this row) ----
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    /// <summary>Numeric on the legacy schema — no support for a non-numeric or ZIP+4 postal code.</summary>
    public int? ZipCode { get; set; }

    /// <summary>Always true in practice — this row IS the primary contact (see class remarks); preserved
    /// as a real column rather than assumed, since it already exists on the live schema.</summary>
    public bool IsPrimaryContact { get; set; } = true;

    public bool IsBillingContact { get; set; }

    public bool IsAuthorizedToPickUpStudent { get; set; }

    /// <summary>Free text on the live schema (not a flag).</summary>
    public string? MassEmailOptOut { get; set; }

    /// <summary>Free text on the live schema (not a flag).</summary>
    public string? TextOptIn { get; set; }

    /// <summary>Legacy duplicate of <see cref="Active"/> of unclear origin — preserved as-is, not written
    /// by new saves.</summary>
    public bool? Active2 { get; set; }

    // ---- Added by ExtendParentsAndParentContactsForFamilies — see class remarks ----
    public string? Source { get; set; }
    public string? ReferralName { get; set; }
    public string? EmergencyContactPerson { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? HealthInsuranceCarrier { get; set; }

    // ---- Navigations ----
    public Tenant? Tenant { get; set; }
    public Location? StudioLocation { get; set; }
    public FamilyStatus? FamilyStatus { get; set; }
    public ICollection<FamilyPersonMapping> Contacts { get; set; } = new List<FamilyPersonMapping>();
}
