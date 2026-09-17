using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for <see cref="ClassCategory"/> records (the Class form's Category 1/2/3 dropdown options).</summary>
public interface IClassCategoryRepository
{
    /// <summary>Every non-deleted category owned by the given tenant, grouped implicitly by <see cref="ClassCategory.CategoryType"/> via the caller.</summary>
    Task<IReadOnlyList<ClassCategory>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
