using Sakolee.Application.Abstractions.Leads;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Leads;

/// <summary>
/// Builds the lead list: students who are not currently enrolled in a running class — a class still
/// flagged Active whose end date is null or not yet past — so a student in no class, or only in
/// inactive/finished ones, reads as a lead.
/// <para>
/// The join is assembled in memory (the Students list does the same): a student's name and email live
/// on the linked <see cref="Person"/>, the household contact and studio location on the
/// <see cref="Family"/> row, and the tenant scope arrives through <see cref="IStudentRepository.ListAsync"/>.
/// </para>
/// </summary>
public sealed class LeadService : ILeadService
{
    private readonly IStudentRepository _students;
    private readonly IClassRepository _classes;
    private readonly IPersonRepository _persons;
    private readonly IFamilyRepository _families;

    public LeadService(
        IStudentRepository students,
        IClassRepository classes,
        IPersonRepository persons,
        IFamilyRepository families)
    {
        _students = students;
        _classes = classes;
        _persons = persons;
        _families = families;
    }

    // What the lead list may be ordered by. Created/Updated By are ids the controller resolves to
    // names after the query, so there is no column to order on.
    private static readonly SortMap<LeadListItem> Sorts = new SortMap<LeadListItem>("studentLastName")
        .Add("studentFirstName", i => i.StudentFirstName)
        .Add("studentLastName", i => i.StudentLastName)
        .Add("email", i => i.Email)
        .Add("contactFirstName", i => i.ContactFirstName)
        .Add("contactLastName", i => i.ContactLastName)
        .Add("familyName", i => i.FamilyName)
        // The UI's Studio Location column is named after its filter key; ordering it orders by the name.
        .Add("studioLocationId", i => i.StudioLocationName)
        .Add("admissionDate", i => i.AdmissionDate)
        .Add("createdOnUtc", i => i.CreatedOnUtc)
        .Add("updatedOnUtc", i => i.UpdatedOnUtc ?? i.CreatedOnUtc);

    public async Task<(IReadOnlyList<LeadListItem> Items, int Total)> ListAsync(
        LeadListQuery query, CancellationToken cancellationToken = default)
    {
        // Every student the tenant owns, then the classes each sits in (StudentClass rows, falling back
        // to the legacy single Student.ClassId).
        var students = await _students.ListAsync(query.TenantId, cancellationToken);
        var classIdsByStudent = await _students.GetClassIdsAsync(students, cancellationToken);

        var running = await RunningClassIdsAsync(cancellationToken);
        IEnumerable<Student> leads = students
            .Where(s => !classIdsByStudent[s.Id].Any(id => running.Contains(id)));

        // Names/email come from the Person; contact and studio location from the Family.
        var personIds = leads.Where(s => s.PersonId.HasValue).Select(s => s.PersonId!.Value).Distinct().ToList();
        var persons = await LoadPersonsAsync(personIds, cancellationToken);

        var familyIds = leads.Where(s => s.FamilyId.HasValue).Select(s => s.FamilyId!.Value).Distinct().ToList();
        var families = await _families.ListByIdsAsync(familyIds, query.TenantId, cancellationToken);
        var familyById = families.ToDictionary(f => f.Id);

        var rows = leads.Select(s =>
        {
            var person = s.PersonId is { } personId && persons.TryGetValue(personId, out var p) ? p : null;
            var family = s.FamilyId is { } familyId && familyById.TryGetValue(familyId, out var f) ? f : null;
            return new LeadListItem(
                s.Id,
                s.PersonId,
                s.FamilyId,
                person?.FirstName,
                person?.LastName,
                person?.PrimaryEmail,
                s.CellPhone ?? person?.MobileNumber,
                family?.FirstName,
                family?.LastName,
                family?.Email,
                family?.CellPhone ?? family?.HomePhone,
                family?.FamilyName ?? s.FamilyName,
                family?.StudioLocationId,
                family?.StudioLocation?.Name,
                s.AdmissionDate,
                s.CreatedById,
                s.CreatedOnUtc,
                s.UpdatedById,
                s.UpdatedOnUtc);
        });

        IEnumerable<LeadListItem> filtered = rows;

        if (query.StudioLocationId is { } locationId)
        {
            filtered = filtered.Where(r => r.StudioLocationId == locationId);
        }

        var studentFirstName = query.StudentFirstName;
        if (!string.IsNullOrWhiteSpace(studentFirstName))
        {
            filtered = filtered.Where(r => Matches(r.StudentFirstName, studentFirstName));
        }

        var studentLastName = query.StudentLastName;
        if (!string.IsNullOrWhiteSpace(studentLastName))
        {
            filtered = filtered.Where(r => Matches(r.StudentLastName, studentLastName));
        }

        var contactFirstName = query.ContactFirstName;
        if (!string.IsNullOrWhiteSpace(contactFirstName))
        {
            filtered = filtered.Where(r => Matches(r.ContactFirstName, contactFirstName));
        }

        var contactLastName = query.ContactLastName;
        if (!string.IsNullOrWhiteSpace(contactLastName))
        {
            filtered = filtered.Where(r => Matches(r.ContactLastName, contactLastName));
        }

        var email = query.Email;
        if (!string.IsNullOrWhiteSpace(email))
        {
            filtered = filtered.Where(r => Matches(r.Email, email) || Matches(r.ContactEmail, email));
        }

        var ordered = Sorts.Apply(filtered, query.SortBy, query.Descending).ToList();
        var items = ordered.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToList();
        return (items, ordered.Count);
    }

    /// <summary>The class ids a student may still be counted as sitting in: flagged Active and not ended.</summary>
    private async Task<HashSet<Guid>> RunningClassIdsAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var classes = await _classes.ListAsync(cancellationToken);
        return classes
            .Where(c => c.Active && (c.EndDate is null || c.EndDate.Value.Date >= today))
            .Select(c => c.Id)
            .ToHashSet();
    }

    private async Task<IReadOnlyDictionary<Guid, Person>> LoadPersonsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => ids.Count == 0
            ? new Dictionary<Guid, Person>()
            : (await _persons.GetByIdsAsync(ids, cancellationToken)).ToDictionary(p => p.Id);

    /// <summary>Case-insensitive containment, the term as the reader typed it.</summary>
    private static bool Matches(string? value, string term)
        => value?.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
