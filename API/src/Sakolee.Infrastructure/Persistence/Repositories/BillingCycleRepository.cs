using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for Billing Cycle records.
/// </summary>
internal sealed class BillingCycleRepository : IBillingCycleRepository
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Billing Cycle repository.
    /// </summary>
    public BillingCycleRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    #endregion

    #region List 

    /// <summary>
    /// Gets all non-deleted Billing Cycles belonging to the specified tenant.
    /// Supports searching by Billing Cycle name.
    /// </summary>
    public async Task<IReadOnlyList<BillingCycle>> ListByTenantAsync(Guid tenantId, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.BillingCycles .Where(x => !x.Deleted && x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }
        var items = await query .OrderBy(x => x.Name) .ToListAsync(cancellationToken);
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants .FirstOrDefaultAsync( x => x.Id == tenantId, cancellationToken);
            foreach (var item in items)
            {
                item.Tenant = tenant;
            }
        }
        return items;
    }
    #endregion

    #region Get
    /// <summary>
    /// Gets a non-deleted Billing Cycle by id for the specified tenant.
    /// </summary>
    public async Task<BillingCycle?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var billingCycle = await _dbContext.BillingCycles.FirstOrDefaultAsync( x => x.Id == id &&  x.TenantId == tenantId && !x.Deleted, cancellationToken);
        if (billingCycle is not null)
        {
            billingCycle.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync( x => x.Id == tenantId, cancellationToken);
        }
        return billingCycle;
    }
    #endregion

    #region ExistsByName
    /// <summary>
    /// Checks whether a non-deleted Billing Cycle with the specified
    /// name already exists for the given tenant.
    /// When excludeId is provided, that record is ignored.
    /// This is required during update so that a Billing Cycle does not
    /// conflict with itself.
    /// </summary>
    public async Task<bool> ExistsByNameAsync( Guid tenantId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var query = _dbContext.BillingCycles.AsNoTracking().Where(x => x.TenantId == tenantId &&  !x.Deleted && x.Name == normalizedName);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Create
    /// <summary>
    /// Adds a Billing Cycle to the current DbContext.
    /// </summary>
    public Task AddAsync(BillingCycle billingCycle, CancellationToken cancellationToken = default) => _dbContext.BillingCycles.AddAsync(billingCycle, cancellationToken).AsTask();
    #endregion

    #region Update
    /// <summary>
    /// Marks a Billing Cycle as modified in the current DbContext.
    /// </summary>
    public void Update(BillingCycle billingCycle) => _dbContext.BillingCycles.Update(billingCycle);
    #endregion

    #region Delete
    /// <summary>
    /// Removes a Billing Cycle from the current DbContext.
    /// </summary>
    public void Remove(BillingCycle billingCycle) => _dbContext.BillingCycles.Remove(billingCycle);
    #endregion
}