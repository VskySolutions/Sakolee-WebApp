using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for the <see cref="Session"/> master record.</summary>
public interface IClassSessionRepository
{
    /// <summary>Loads a session by id, scoped to the active tenant.</summary>
    Task<ClassSessions?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a session ignoring the tenant filter — for cross-tenant Super Admin access.</summary>
    Task<ClassSessions?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Batch-loads sessions by id, scoped to the active tenant.</summary>
    Task<IReadOnlyList<ClassSessions>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Checks if a session with the given name already exists.</summary>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated list with optional free-text search (name) and optional
    /// structured filters (owning tenant, active state).
    /// </summary>
    Task<(IReadOnlyList<ClassSessions> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, bool? isActive, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    /// <summary>Lightweight selection list for dropdowns.</summary>
    Task<IReadOnlyList<ClassSessions>> ListSelectableAsync(
        Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task AddAsync(ClassSessions session, CancellationToken cancellationToken = default);

    void Update(ClassSessions session);

    /// <summary>Soft-deletes the session record.</summary>
    void Remove(ClassSessions session);
}