using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for <see cref="Class"/> records. Not tenant-scoped — see the entity remarks.</summary>
public interface IClassRepository
{
    Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every non-deleted class, newest first.</summary>
    Task<IReadOnlyList<Class>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Class @class, CancellationToken cancellationToken = default);

    void Update(Class @class);

    Task<string?> ValidateInstructorScheduleAsync(
                                                    Guid? classId,
                                                    Guid? primaryInstructorId,
                                                    IEnumerable<Guid>? additionalInstructorIds,
                                                    DateTime? startDate,
                                                    DateTime? endDate,
                                                    string startTime,
                                                    string endTime,
                                                    string? activeDays,
                                                    CancellationToken cancellationToken = default);
}
