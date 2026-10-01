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
    public async Task<(IReadOnlyList<BillingCycle> Items, int Total)> ListByTenantAsync(Guid tenantId, string? search = null, string? name = null, bool showDeleted = false,bool? active = null, string? sortBy = null, bool descending = false, int page = 1, int limit = 20, CancellationToken cancellationToken = default)
    {
        // Get Billing Cycles for the current tenant.
        // When showDeleted is true, bypass the global soft-delete filter.
        var query = showDeleted? _dbContext.BillingCycles.IgnoreQueryFilters().Where(x => x.TenantId == tenantId) : _dbContext.BillingCycles.Where(x => x.TenantId == tenantId);
        // Show only non-deleted records by default.
        if (!showDeleted)
        {
            query = query.Where(x => !x.Deleted);
        }

        // Filter by Active/Inactive when requested.
        if (active.HasValue)
        {
            query = query.Where(x => x.Active == active.Value);
        }
        // Apply name filter when a name is provided.
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            query = query.Where(x => x.Name.Contains(name));
        }
        // Apply search filter when a search value is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }
        // Get total records before pagination.
        var total = await query.CountAsync(cancellationToken);
        // Apply sorting based on the requested column.
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "active" => descending ? query.OrderByDescending(x => x.Active) : query.OrderBy(x => x.Active),
            "createdonutc" => descending ? query.OrderByDescending(x => x.CreatedOnUtc) : query.OrderBy(x => x.CreatedOnUtc),
            "updatedonutc" => descending ? query.OrderByDescending(x => x.UpdatedOnUtc) : query.OrderBy(x => x.UpdatedOnUtc),
            //_ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
            // Default: show the most recently created or updated record first.
            _ => query.OrderByDescending(x => x.UpdatedOnUtc > x.CreatedOnUtc ? x.UpdatedOnUtc : x.CreatedOnUtc)
        };
        // Execute the query and get the records.Apply server-side pagination.
        var items = await query .Skip((page - 1) * limit) .Take(limit) .ToListAsync(cancellationToken);
        // Load tenant details for the returned records.
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants .FirstOrDefaultAsync( x => x.Id == tenantId, cancellationToken);
            foreach (var item in items)
            {
                item.Tenant = tenant;
            }
        }
        return (items, total);
    }
    #endregion

    #region Get
    /// <summary>
    /// Gets a non-deleted Billing Cycle by id for the specified tenant.
    /// </summary>
    public async Task<BillingCycle?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Get the Billing Cycle only if it belongs to the tenant and is not deleted.
        var billingCycle = await _dbContext.BillingCycles.FirstOrDefaultAsync( x => x.Id == id &&  x.TenantId == tenantId && !x.Deleted, cancellationToken);
        // Load tenant details when the record exists.
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
        // Trim the name before checking for duplicates.
        var normalizedName = name.Trim();
        // Check for an existing non-deleted Billing Cycle with the same name.
        var query = _dbContext.BillingCycles.AsNoTracking().Where(x => x.TenantId == tenantId &&  !x.Deleted && x.Name == normalizedName);
        // Exclude the current record when checking during an update.
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        // Return true when a matching record exists.
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