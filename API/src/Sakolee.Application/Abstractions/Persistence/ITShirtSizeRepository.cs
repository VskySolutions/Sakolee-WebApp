using Sakolee.Domain.Entities;
namespace Sakolee.Application.Abstractions.Persistence;
/// <summary>
/// Data access for T-Shirt Size records.
/// </summary>
public interface ITShirtSizeRepository
{
    /// <summary>
    /// Gets every non-deleted T-Shirt Size owned by the given tenant.
    /// Supports searching by T-Shirt Size name.
    /// </summary>
    Task<IReadOnlyList<TShirtSize>> ListByTenantAsync(Guid tenantId,string? search = null,CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a non-deleted T-Shirt Size by id belonging to the specified tenant.
    /// </summary>
    Task<TShirtSize?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a non-deleted T-Shirt Size with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a T-Shirt Size to the current DbContext.
    /// </summary>
    Task AddAsync(TShirtSize tShirtSize,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a T-Shirt Size as modified in the current DbContext.
    /// </summary>
    void Update(TShirtSize tShirtSize);

    /// <summary>
    /// Removes a T-Shirt Size from the current DbContext.
    /// </summary>
    void Remove(TShirtSize tShirtSize);
}