namespace Sakolee.Domain.Entities;

/// <summary>
/// A student record. Not an <see cref="AuditableEntity"/> — the physical <c>Students</c> table carries
/// only a subset of that shape: no <c>DeletedOnUtc</c>, and <c>UpdatedOnUtc</c>/<c>UpdatedById</c> are
/// nullable rather than required. Audit fields are stamped explicitly by <c>StudentsController</c>
/// instead of the DbContext's generic <see cref="AuditableEntity"/> interceptor.
/// <para>
/// Tenant-scoped indirectly through <see cref="PersonId"/>: a student's owning tenant is whichever
/// tenant the linked Person's <see cref="TenantPersonMapping"/> names, the same indirection Person
/// itself uses. A null (or foreign-tenant) PersonId has no resolvable tenant and is treated as
/// inaccessible once a tenant is resolved — see <c>StudentRepository</c>.
/// </para>
/// <para>
/// Carries no name or email of its own — those live once, on the linked <see cref="Entities.Person"/>
/// (and, from there, the login account <c>StudentsController</c> creates alongside every student). A
/// second copy here was two answers to "what is this student's name" that could (and did) drift apart.
/// </para>
/// </summary>
public class Student
{
    public Guid Id { get; set; }

    /// <summary>Owning parent (Parents table — no Domain entity for it yet).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The linked CRM Person — every student created through <c>StudentsController</c> has one (minted
    /// alongside it). Drives tenant scoping — see class remarks.
    /// </summary>
    public Guid? PersonId { get; set; }

    public string? StudentNumber { get; set; }

    public DateTime? AdmissionDate { get; set; }

    /// <summary>Owning class (no Class management feature yet).</summary>
    public Guid? ClassId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public Guid? UpdatedById { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    public decimal? FeeAmount { get; set; }

    /// <summary>Maps to the physical "FeeExcpirydate" column (typo preserved from the live schema).</summary>
    public DateTime? FeeExpiryDate { get; set; }

    public string? FeeNote { get; set; }

    /// <summary>Owning fee category (no Fee Category management feature yet).</summary>
    public Guid? FeeCategoryId { get; set; }

    public string? FamilyName { get; set; }

    /// <summary>Legacy flag, superseded by <see cref="Entities.Person.Gender"/> (the current student form's
    /// Gender select writes there instead) — preserved as-is, no longer written by new saves.</summary>
    public bool? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? CellPhone { get; set; }

    public string? School { get; set; }

    public string? GradeLevel { get; set; }

    public string? Transportation { get; set; }

    public string? TShirtSize { get; set; }

    /// <summary>Legacy flag, superseded by <see cref="DisabilitiesNotes"/> — preserved as-is, no longer
    /// written by new saves.</summary>
    public bool? Disabilities { get; set; }

    public string? SpecialNeeds { get; set; }

    /// <summary>Legacy flag, superseded by <see cref="AllergiesNotes"/> — preserved as-is, no longer
    /// written by new saves.</summary>
    public bool? Allergies { get; set; }

    public string? Medications { get; set; }

    public string? PrimaryDoctor { get; set; }

    /// <summary>Whether immunizations are up to date: "Yes", "No", or "Exempt".</summary>
    public string? HasImmunizations { get; set; }

    public string? ImmunizationNotes { get; set; }

    public string? SkillNotes { get; set; }

    /// <summary>Free text on the live schema, not a flag — preserved as-is.</summary>
    public string? TextOptIn { get; set; }

    /// <summary>Free text on the live schema, not a flag — preserved as-is.</summary>
    public string? MassEmailOptOut { get; set; }

    public string? HealthInsuranceCarrier { get; set; }

    /// <summary>Free-text description of any disabilities (see <see cref="Disabilities"/> remarks).</summary>
    public string? DisabilitiesNotes { get; set; }

    /// <summary>Free-text description of any allergies (see <see cref="Allergies"/> remarks).</summary>
    public string? AllergiesNotes { get; set; }

    public bool AllowTextMessaging { get; set; } = true;
}
