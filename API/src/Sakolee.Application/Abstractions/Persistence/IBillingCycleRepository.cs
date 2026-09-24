using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Data access for <see cref="BillingCycle"/> records.
/// </summary>
public interface IBillingCycleRepository
{
    /// <summary>
    /// Gets every non-deleted Billing Cycle owned by the given tenant.
    /// Supports searching by Billing Cycle name.
    /// </summary>
    Task<IReadOnlyList<BillingCycle>> ListByTenantAsync(Guid tenantId, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a non-deleted Billing Cycle by id belonging to the specified tenant.
    /// </summary>
    Task<BillingCycle?> GetByIdAsync(Guid id ,Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a non-deleted Billing Cycle with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync( Guid tenantId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a Billing Cycle to the current DbContext.
    /// </summary>
    Task AddAsync(BillingCycle billingCycle,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a Billing Cycle as modified in the current DbContext.
    /// </summary>
    void Update(BillingCycle billingCycle);

    /// <summary>
    /// Removes a Billing Cycle from the current DbContext.
    /// </summary>
    void Remove(BillingCycle billingCycle);
}