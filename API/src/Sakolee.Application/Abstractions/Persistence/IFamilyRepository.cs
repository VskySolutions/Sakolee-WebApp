using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for the <see cref="Family"/> household record (the <c>Families</c> table) and its
/// additional contacts (<see cref="FamilyPersonMapping"/>, the <c>FamilyPersonMapping</c> table).</summary>
public interface IFamilyRepository
{
    /// <summary>Loads a family (with its additional contacts) by id, scoped to the active tenant.</summary>
    Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a family ignoring the tenant filter — for cross-tenant Super Admin access.</summary>
    Task<Family?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> FamilyNameExistsAsync(Guid tenantId, string familyName, Guid? excludingFamilyId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated list with optional free-text search (family name) and optional structured filters
    /// (owning tenant, family status).
    /// </summary>
    Task<(IReadOnlyList<Family> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, Guid? familyStatusId, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    Task AddAsync(Family family, CancellationToken cancellationToken = default);

    void Update(Family family);

    /// <summary>Soft-deletes the family record.</summary>
    void Remove(Family family);

    // ---- Additional contacts ----

    Task<IReadOnlyList<FamilyPersonMapping>> ListContactsAsync(Guid familyId, CancellationToken cancellationToken = default);

    Task AddContactAsync(FamilyPersonMapping contact, CancellationToken cancellationToken = default);

    void UpdateContact(FamilyPersonMapping contact);
}
