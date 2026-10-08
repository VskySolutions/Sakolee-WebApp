namespace Sakolee.Api.Models.Leads;

/// <summary>One lead row: a student who is not in a running class, with the household contact that
/// reaches them. Names/email come from the student's linked Person; the contact and studio location
/// from the family row.</summary>
public sealed record LeadSummary(
    Guid StudentId,
    Guid? PersonId,
    Guid? FamilyId,
    string? StudentFirstName,
    string? StudentLastName,
    string? Email,
    string? CellPhone,
    string? ContactFirstName,
    string? ContactLastName,
    string? ContactEmail,
    string? ContactPhone,
    string? FamilyName,
    Guid? StudioLocationId,
    string? StudioLocationName,
    DateTime? AdmissionDate,
    string? CreatedBy,
    DateTime CreatedOnUtc,
    string? UpdatedBy,
    DateTime? UpdatedOnUtc);
