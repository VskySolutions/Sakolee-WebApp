using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for the <see cref="ClassRooms"/> record (the <c>ClassRooms</c> table).</summary>
public interface IClassRoomRepository
{
    /// <summary>Loads a class room by id, scoped to the active tenant.</summary>
    Task<ClassRooms?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a class room ignoring the tenant filter — for cross-tenant Super Admin access.</summary>
    Task<ClassRooms?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Checks if a class room name already exists within the tenant.</summary>
    Task<bool> ClassRoomNameExistsAsync(
     Guid tenantId,
     Guid locationId,
     string name,
     Guid? excludingClassRoomId = null,
     CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated list with optional free-text search (class room name) and optional structured filters
    /// (owning tenant, location id).
    /// </summary>
    Task<(IReadOnlyList<ClassRooms> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, Guid? locationId, bool? showDeleted, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    Task AddAsync(ClassRooms classRoom, CancellationToken cancellationToken = default);

    void Update(ClassRooms classRoom);

    /// <summary>Soft-deletes the class room record.</summary>
    void Remove(ClassRooms classRoom);
}