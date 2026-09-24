namespace Sakolee.Api.Models.Families;

/// <summary>
/// The (optional) secondary contact on create/update — a row on <c>ParentContacts</c>. The primary
/// contact has no equivalent nested shape: it is inlined directly on <c>CreateFamilyRequest</c>/
/// <c>UpdateFamilyRequest</c>, because the physical <c>Parents</c> row already inlines it (see the
/// <c>Family</c> entity's remarks).
/// </summary>
public sealed class FamilyContactInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>Required — every contact gets a login account, and an account needs an email.</summary>
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    /// <summary>Free-text relationship to the student(s), e.g. "Mother", "Father", "Guardian".</summary>
    public string? Relation { get; set; }
    public bool IsBillingContact { get; set; }
    public bool IsAuthorizedToPickUpStudent { get; set; }
}

/// <summary>
/// Request to create a family. Mints the Family (<c>Parents</c>) row plus a CRM Person (and login
/// account) for the primary contact, and for the secondary contact when supplied — mirrors how
/// <c>StudentsController.Create</c> mints a Person for a new student.
/// </summary>
public sealed class CreateFamilyRequest
{
    public string FamilyName { get; set; } = string.Empty;
    public Guid? StudioLocationId { get; set; }
    public Guid? FamilyStatusId { get; set; }
    public string? Source { get; set; }
    public string? ReferralName { get; set; }

    // ---- Primary contact (inlined on the Family row) ----
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>Required — every family gets a login account for its primary contact.</summary>
    public string Email { get; set; } = string.Empty;
    public string? Relation { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? HomePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? CellPhone { get; set; }
    public string? Fax { get; set; }
    public string? OtherPhone { get; set; }
    public bool IsBillingContact { get; set; } = true;
    public bool IsAuthorizedToPickUpStudent { get; set; } = true;

    // ---- Household address ----
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int? ZipCode { get; set; }

    public string? EmergencyContactPerson { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? HealthInsuranceCarrier { get; set; }

    public FamilyContactInput? SecondaryContact { get; set; }
}

/// <summary>
/// Request to update a family. Family-level fields are optional (patch semantics, mirrors
/// <c>UpdateStudentRequest</c>). A supplied <see cref="SecondaryContact"/> patches the existing one, or
/// mints a new one if the family has none yet.
/// </summary>
public sealed class UpdateFamilyRequest
{
    public string? FamilyName { get; set; }
    public Guid? StudioLocationId { get; set; }
    public Guid? FamilyStatusId { get; set; }
    public string? Source { get; set; }
    public string? ReferralName { get; set; }
    public bool Active { get; set; } = true;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Relation { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? HomePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? CellPhone { get; set; }
    public string? Fax { get; set; }
    public string? OtherPhone { get; set; }
    public bool IsBillingContact { get; set; }
    public bool IsAuthorizedToPickUpStudent { get; set; }

    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int? ZipCode { get; set; }

    public string? EmergencyContactPerson { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? HealthInsuranceCarrier { get; set; }

    public FamilyContactInput? SecondaryContact { get; set; }
}

/// <summary>A contact as returned after create/update. <see cref="TemporaryPassword"/> is populated only
/// the moment a new login account is minted for this contact (mirrors <c>StudentResponse</c>).
/// <see cref="ContactId"/> is the contact's <c>FamilyPersonMapping</c> row id — every contact, primary
/// or secondary, has one (see that entity's remarks) — and is null only for a family predating that row
/// existing for its primary contact.</summary>
public sealed record FamilyContactResponse(
    Guid? ContactId, Guid? PersonId, string FirstName, string LastName, string? Email, string? Phone,
    string? Relation, bool IsPrimaryContact, bool IsBillingContact, bool IsAuthorizedToPickUpStudent,
    string? TemporaryPassword);

/// <summary>A family as returned after create/update.</summary>
public sealed record FamilyResponse(Guid FamilyId, string? FamilyName, IReadOnlyList<FamilyContactResponse> Contacts);

/// <summary>A student enrolled under a family — the minimal projection a family's detail view needs
/// (full detail lives on the Students screen).</summary>
public sealed record FamilyStudentSummary(
    Guid StudentId, string? FirstName, string? LastName, string? StudentNumber, bool Active, Guid? ClassId);

/// <summary>Full family detail: its own fields, every contact, and every enrolled student.</summary>
public sealed record FamilyDetail(
    Guid FamilyId,
    string? FamilyName,
    Guid? StudioLocationId,
    string? StudioLocationName,
    Guid? FamilyStatusId,
    string? FamilyStatusName,
    string? Source,
    string? ReferralName,
    string? HomePhone,
    string? WorkPhone,
    string? Fax,
    string? OtherPhone,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    int? ZipCode,
    string? EmergencyContactPerson,
    string? EmergencyPhone,
    string? HealthInsuranceCarrier,
    bool Active,
    IReadOnlyList<FamilyContactResponse> Contacts,
    IReadOnlyList<FamilyStudentSummary> Students,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);

/// <summary>A family row for the list page.</summary>
public sealed record FamilySummary(
    Guid FamilyId,
    string? FamilyName,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone,
    int ContactCount,
    int StudentCount,
    Guid? StudioLocationId,
    string? StudioLocationName,
    Guid? FamilyStatusId,
    string? FamilyStatusName,
    bool Active,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);
