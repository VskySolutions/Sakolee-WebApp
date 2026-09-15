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
    public bool? Gender { get; set; }
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
    public string? ImmunizationNotes { get; set; }
    public string? SkillNotes { get; set; }
    public string? TextOptIn { get; set; }
    public string? MassEmailOptOut { get; set; }
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
    public bool? Gender { get; set; }
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
    public string? ImmunizationNotes { get; set; }
    public string? SkillNotes { get; set; }
    public string? TextOptIn { get; set; }
    public string? MassEmailOptOut { get; set; }
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
    bool? Gender,
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
    string? ImmunizationNotes,
    string? SkillNotes,
    string? TextOptIn,
    string? MassEmailOptOut,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);
