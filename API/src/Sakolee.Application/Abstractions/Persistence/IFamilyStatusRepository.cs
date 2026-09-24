using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for the <see cref="FamilyStatus"/> master record.</summary>
public interface IFamilyStatusRepository
{
    /// <summary>Loads a family status by id, scoped to the active tenant.</summary>
    Task<FamilyStatus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a family status ignoring the tenant filter — for cross-tenant Super Admin access.</summary>
    Task<FamilyStatus?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Batch-loads family statuses by id, scoped to the active tenant.</summary>
    Task<IReadOnlyList<FamilyStatus>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Checks if a family status with the given name already exists.</summary>
    // Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated list with optional free-text search (name) and optional
    /// structured filters (owning tenant, active state).
    /// </summary>
    Task<(IReadOnlyList<FamilyStatus> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, bool? isActive, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    /// <summary>Lightweight selection list for dropdowns.</summary>
    Task<IReadOnlyList<FamilyStatus>> ListSelectableAsync(
        Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task AddAsync(FamilyStatus familyStatus, CancellationToken cancellationToken = default);

    void Update(FamilyStatus familyStatus);

    /// <summary>Soft-deletes the family status record.</summary>
    void Remove(FamilyStatus familyStatus);
}