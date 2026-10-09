namespace Sakolee.Application.Abstractions.Leads;

/// <summary>
/// The lead list: students of the caller's tenant who are not currently sitting in a running class
/// (see <c>LeadService</c> for what "running" means), filtered, sorted, and paged for the Lead Files
/// screen under Families.
/// </summary>
public interface ILeadService
{
    /// <summary>Returns the matching lead rows for the current page, plus the total number of matches.</summary>
    Task<(IReadOnlyList<LeadListItem> Items, int Total)> ListAsync(
        LeadListQuery query, CancellationToken cancellationToken = default);
}

/// <summary>Everything the lead list filters, sorts, and pages by — the tenant scopes which students
/// are considered, the rest narrow the joined rows.</summary>
public sealed record LeadListQuery(
    Guid? TenantId,
    Guid? StudioLocationId,
    string? ContactFirstName,
    string? ContactLastName,
    string? StudentFirstName,
    string? StudentLastName,
    string? Email,
    string? SortBy,
    bool Descending,
    int Page,
    int Limit);

/// <summary>One lead row: the student, their linked Person (name/email), and their household's contact.</summary>
public sealed record LeadListItem(
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
    Guid? CreatedById,
    DateTime CreatedOnUtc,
    Guid? UpdatedById,
    DateTime? UpdatedOnUtc);
