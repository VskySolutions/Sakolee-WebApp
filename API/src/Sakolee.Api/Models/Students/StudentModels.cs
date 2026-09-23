using Sakolee.Api.Models.Profile;

namespace Sakolee.Api.Models.Students;

/// <summary>
/// Request to create a student. Unlike Person or User, there is no existing record to link to here —
/// creating a student always mints a new CRM Person (from the identity fields below) and a new login
/// account for it, carrying the <c>Student</c> role (see <c>StudentsController.Create</c>).
/// </summary>
public sealed class CreateStudentRequest
{
    public Guid? ParentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>Required — every student gets a login account, and an account needs an email.</summary>
    public string Email { get; set; } = string.Empty;
    public string? FamilyName { get; set; }
    public string? StudentNumber { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public Guid? ClassId { get; set; }
    public decimal? FeeAmount { get; set; }
    public DateTime? FeeExpiryDate { get; set; }
    public string? FeeNote { get; set; }
    public Guid? FeeCategoryId { get; set; }
    /// <summary>Writes to the linked Person's Gender — see <see cref="Sakolee.Domain.Entities.Student.Gender"/> remarks.</summary>
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? CellPhone { get; set; }
    public string? School { get; set; }
    public string? GradeLevel { get; set; }
    public string? Transportation { get; set; }
    public string? TShirtSize { get; set; }
    public bool? Disabilities { get; set; }
    public string? SpecialNeeds { get; set; }
    public bool? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? PrimaryDoctor { get; set; }
    public string? HasImmunizations { get; set; }
    public string? ImmunizationNotes { get; set; }
    public string? SkillNotes { get; set; }
    public string? TextOptIn { get; set; }
    public string? MassEmailOptOut { get; set; }
    public string? HealthInsuranceCarrier { get; set; }
    public string? DisabilitiesNotes { get; set; }
    public string? AllergiesNotes { get; set; }
    public bool AllowTextMessaging { get; set; } = true;
    /// <summary>Writes to the linked Person's EmergencyContactName.</summary>
    public string? EmergencyContactName { get; set; }
    /// <summary>Writes to the linked Person's EmergencyContactNumber.</summary>
    public string? EmergencyContactNumber { get; set; }
    /// <summary>Writes to the linked Person's Address — same shape as <c>CreatePersonRequest.Address</c>.</summary>
    public AddressInput? Address { get; set; }
}

/// <summary>
/// Request to update a student. <see cref="FirstName"/>/<see cref="LastName"/>/<see cref="Email"/> are
/// optional here (unlike on create) and, when supplied, patch the linked Person (and, for Email, the
/// linked login account) rather than the Student row itself — Student carries no copy of its own.
/// </summary>
public sealed class UpdateStudentRequest
{
    public Guid? ParentId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? FamilyName { get; set; }
    public string? StudentNumber { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public Guid? ClassId { get; set; }
    public bool Active { get; set; } = true;
    public decimal? FeeAmount { get; set; }
    public DateTime? FeeExpiryDate { get; set; }
    public string? FeeNote { get; set; }
    public Guid? FeeCategoryId { get; set; }
    /// <summary>Writes to the linked Person's Gender — see <see cref="Sakolee.Domain.Entities.Student.Gender"/> remarks.</summary>
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? CellPhone { get; set; }
    public string? School { get; set; }
    public string? GradeLevel { get; set; }
    public string? Transportation { get; set; }
    public string? TShirtSize { get; set; }
    public bool? Disabilities { get; set; }
    public string? SpecialNeeds { get; set; }
    public bool? Allergies { get; set; }
    public string? Medications { get; set; }
    public string? PrimaryDoctor { get; set; }
    public string? HasImmunizations { get; set; }
    public string? ImmunizationNotes { get; set; }
    public string? SkillNotes { get; set; }
    public string? TextOptIn { get; set; }
    public string? MassEmailOptOut { get; set; }
    public string? HealthInsuranceCarrier { get; set; }
    public string? DisabilitiesNotes { get; set; }
    public string? AllergiesNotes { get; set; }
    public bool AllowTextMessaging { get; set; } = true;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public AddressInput? Address { get; set; }
}

/// <summary>
/// A student as returned after create/update. <see cref="TemporaryPassword"/> is populated only on
/// create (the one time it is ever shown in plaintext — mirrors <c>CreateUserResponse</c>).
/// </summary>
public sealed record StudentResponse(
    Guid StudentId, Guid? PersonId, Guid? UserId, string? FirstName, string? LastName, bool Active,
    string? TemporaryPassword);

/// <summary>A student row, full detail. Identity fields (name, email) are joined in from the linked Person.</summary>
public sealed record StudentSummary(
    Guid StudentId,
    Guid? PersonId,
    Guid? ParentId,
    string? FirstName,
    string? LastName,
    string? FamilyName,
    string? StudentNumber,
    DateTime? AdmissionDate,
    Guid? ClassId,
    bool Active,
    decimal? FeeAmount,
    DateTime? FeeExpiryDate,
    string? FeeNote,
    Guid? FeeCategoryId,
    string? Gender,
    DateTime? BirthDate,
    string? CellPhone,
    string? Email,
    string? School,
    string? GradeLevel,
    string? Transportation,
    string? TShirtSize,
    bool? Disabilities,
    string? SpecialNeeds,
    bool? Allergies,
    string? Medications,
    string? PrimaryDoctor,
    string? HasImmunizations,
    string? ImmunizationNotes,
    string? SkillNotes,
    string? TextOptIn,
    string? MassEmailOptOut,
    string? HealthInsuranceCarrier,
    string? DisabilitiesNotes,
    string? AllergiesNotes,
    bool AllowTextMessaging,
    string? EmergencyContactName,
    string? EmergencyContactNumber,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);
