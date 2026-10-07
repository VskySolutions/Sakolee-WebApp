using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class ClassRepository : IClassRepository
{
    private readonly SakoleeDbContext _dbContext;

    public ClassRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == id && !c.Deleted, cancellationToken);

    public async Task<IReadOnlyList<Class>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Classes
            .Where(c => !c.Deleted)
            .OrderByDescending(c => c.UpdatedOnUtc ?? c.CreatedOnUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Class @class, CancellationToken cancellationToken = default)
        => await _dbContext.Classes.AddAsync(@class, cancellationToken);

    public void Update(Class @class) => _dbContext.Classes.Update(@class);

    #region validate Instructor Schedule
    public async Task<string?> ValidateInstructorScheduleAsync(
                                                                Guid? classId,
                                                                Guid? primaryInstructorId,
                                                                IEnumerable<Guid>? additionalInstructorIds,
                                                                DateTime? startDate,
                                                                DateTime? endDate,
                                                                string? startTime,
                                                                string? endTime,
                                                                string? activeDays,
                                                                CancellationToken cancellationToken = default)
    {
        var instructorIds = new HashSet<Guid>();

        // Add primary instructor
        if (primaryInstructorId.HasValue && primaryInstructorId.Value != Guid.Empty)
        {
            instructorIds.Add(primaryInstructorId.Value);
        }

        // Add additional instructors
        if (additionalInstructorIds != null)
        {
            foreach (var instructorId in additionalInstructorIds)
            {
                if (instructorId != Guid.Empty)
                {
                    instructorIds.Add(instructorId);
                }
            }
        }

        // No instructor selected, nothing to validate.
        if (instructorIds.Count == 0)
        {
            return null;
        }

        // Validate required date/time values.
        if (!startDate.HasValue || !endDate.HasValue ||
            string.IsNullOrWhiteSpace(startTime) ||
            string.IsNullOrWhiteSpace(endTime))
        {
            return null;
        }

        // Get classes whose date ranges overlap.
        var query = _dbContext.Classes
            .Where(x =>
                x.Active &&
                !x.Deleted &&
                x.StartDate.HasValue &&
                x.EndDate.HasValue &&
                x.StartDate.Value <= endDate.Value &&
                x.EndDate.Value >= startDate.Value);

        // IMPORTANT:
        // During Edit, exclude the current class.
        if (classId.HasValue)
        {
            query = query.Where(x => x.Id != classId.Value);
        }

        var existingClasses = await query
            .Select(x => new
            {
                x.Id,
                x.ClassName,
                x.PrimaryInstructorId,
                x.AdditionalInstructors,
                x.StartDate,
                x.EndDate,
                x.StartTime,
                x.EndTime,
                x.ActiveDays
            })
            .ToListAsync(cancellationToken);

        foreach (var existingClass in existingClasses)
        {
            var existingInstructorIds = new HashSet<Guid>();

            // Primary instructor
            if (existingClass.PrimaryInstructorId.HasValue &&
                existingClass.PrimaryInstructorId.Value != Guid.Empty)
            {
                existingInstructorIds.Add(existingClass.PrimaryInstructorId.Value);
            }

            // Additional instructors
            if (!string.IsNullOrWhiteSpace(existingClass.AdditionalInstructors))
            {
                foreach (var instructorId in existingClass.AdditionalInstructors
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Guid.TryParse(instructorId.Trim(), out var parsedInstructorId))
                    {
                        existingInstructorIds.Add(parsedInstructorId);
                    }
                }
            }

            // No common instructor.
            if (!existingInstructorIds.Overlaps(instructorIds))
            {
                continue;
            }

            // Check time overlap.
            if (!TimeRangesOverlap(
                    existingClass.StartTime,
                    existingClass.EndTime,
                    startTime,
                    endTime))
            {
                continue;
            }

            // Check active-day overlap.
            if (!ActiveDaysOverlap(
                    existingClass.ActiveDays,
                    activeDays))
            {
                continue;
            }

            return $"Selected instructor already exists in date & time for class '{existingClass.ClassName}'.";
        }

        return null;
    }

    public async Task<IReadOnlyList<Class>> ListByInstructorIdAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var instructorIdText = instructorId.ToString();

        return await _dbContext.Classes.Where(c => !c.Deleted &&
                (
                    c.PrimaryInstructorId == instructorId
                    ||
                    (
                        !string.IsNullOrWhiteSpace(c.AdditionalInstructors) &&
                        ("," + c.AdditionalInstructors + ",")
                            .Contains("," + instructorIdText + ",")
                    )
                ))
            .OrderBy(c => c.ClassName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountEnrollmentsByClassIdsAsync(IEnumerable<Guid> classIds, CancellationToken cancellationToken = default)
    {
        var ids = classIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await _dbContext.StudentClasses.Where(sc => !sc.Deleted && ids.Contains(sc.ClassId)).GroupBy(sc => sc.ClassId).ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    private static bool TimeRangesOverlap(string? existingStartTime, string? existingEndTime, string? newStartTime, string? newEndTime)
    {
        if (!DateTime.TryParse(existingStartTime, out var existingStart) ||
            !DateTime.TryParse(existingEndTime, out var existingEnd) ||
            !DateTime.TryParse(newStartTime, out var newStart) ||
            !DateTime.TryParse(newEndTime, out var newEnd))
        {
            return false;
        }

        return existingStart.TimeOfDay < newEnd.TimeOfDay &&
               existingEnd.TimeOfDay > newStart.TimeOfDay;
    }

    private static bool ActiveDaysOverlap(string? existingDays, string? newDays)
    {
        if (string.IsNullOrWhiteSpace(existingDays) || string.IsNullOrWhiteSpace(newDays))
        {
            return true;
        }

        var existing = existingDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var selected = newDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x));

        return selected.Any(existing.Contains);
    }
    #endregion
}
