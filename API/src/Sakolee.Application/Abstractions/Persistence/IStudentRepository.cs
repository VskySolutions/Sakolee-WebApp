using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for <see cref="Student"/> records, tenant-scoped through PersonId.</summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every student mapped (via PersonId) to the given tenant, or every student when
    /// <paramref name="tenantId"/> is null (no tenant resolved — background/global operations).</summary>
    Task<IReadOnlyList<Student>> ListAsync(Guid? tenantId, CancellationToken cancellationToken = default);

    /// <summary>Every (non-deleted) student linked to a family, via <see cref="Student.FamilyId"/> — the
    /// FK onto <c>Family.Id</c> (see <see cref="Student.FamilyId"/> remarks). Feeds a family's detail
    /// view.</summary>
    Task<IReadOnlyList<Student>> ListByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);

    /// <summary>Batch count of (non-deleted) students per family id — feeds the Families list's student
    /// count column without an N+1 query per row.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByFamilyIdsAsync(IEnumerable<Guid> familyIds, CancellationToken cancellationToken = default);

    /// <summary>Whether <paramref name="personId"/> is mapped to <paramref name="tenantId"/> — the
    /// ownership check a caller runs before returning or mutating a specific student.</summary>
    Task<bool> IsOwnedByTenantAsync(Guid? personId, Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    void Update(Student student);
}
