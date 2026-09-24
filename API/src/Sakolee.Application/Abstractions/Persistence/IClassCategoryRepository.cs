using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Data access for <see cref="ClassCategory"/> records.
/// </summary>
public interface IClassCategoryRepository
{
    /// <summary>
    /// Gets every non-deleted category owned by the given tenant.
    /// Supports searching by category name or category type.
    /// </summary>
    Task<IReadOnlyList<ClassCategory>> ListByTenantAsync(Guid tenantId,string? search = null,CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a non-deleted category by id belonging to the specified tenant.
    /// </summary>
    Task<ClassCategory?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default);

    /// <summary>
    /// Names of the given (non-deleted) categories, keyed by id — lets a class show its saved Category
    /// 1/2/3 by name. Not tenant-filtered: the ids come from the class itself.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a non-deleted Class Category with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a class category to the current DbContext.
    /// </summary>
    Task AddAsync(ClassCategory classCategory,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a class category as modified in the current DbContext.
    /// </summary>
    void Update(ClassCategory classCategory);

    /// <summary>
    /// Removes a class category from the current DbContext.
    /// </summary>
    void Remove(ClassCategory classCategory);
}