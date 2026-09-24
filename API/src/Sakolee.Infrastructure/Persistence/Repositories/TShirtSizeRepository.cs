using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for T-Shirt Size records.
/// </summary>
internal sealed class TShirtSizeRepository : ITShirtSizeRepository
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the T-Shirt Size repository.
    /// </summary>
    public TShirtSizeRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    #endregion

    #region List

    /// <summary>
    /// Gets all non-deleted T-Shirt Sizes belonging to the specified tenant.
    /// Supports searching by T-Shirt Size name.
    /// </summary>
    public async Task<IReadOnlyList<TShirtSize>> ListByTenantAsync(Guid tenantId, string? search = null,CancellationToken cancellationToken = default)
    {
        // Build the query to retrieve only active T-Shirt Sizes for the specified tenant.
        var query = _dbContext.TShirtSizes .Where(x => !x.Deleted && x.TenantId == tenantId);
        // Apply the search filter when a search value is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }
        // Execute the query and order the results by T-Shirt Size name.
        var items = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        // Load the tenant information and associate it with each T-Shirt Size.
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId,cancellationToken);
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
    /// Gets a non-deleted T-Shirt Size by id for the specified tenant.
    /// </summary>
    public async Task<TShirtSize?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default)
    {
        // Find the T-Shirt Size by its identifier and tenant while excluding deleted records.
        var tShirtSize = await _dbContext.TShirtSizes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.Deleted, cancellationToken);
        // Load and associate the tenant when the T-Shirt Size exists.
        if (tShirtSize is not null)
        {
            tShirtSize.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId,cancellationToken);
        }
        return tShirtSize;
    }

    #endregion

    #region ExistsByName

    /// <summary>
    /// Checks whether a non-deleted T-Shirt Size with the specified
    /// name already exists for the given tenant.
    /// When excludeId is provided, that record is ignored.
    /// </summary>
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default)
    {
        // Normalize the provided name before performing the duplicate check.
        var normalizedName = name.Trim();
        // Build a query for active T-Shirt Sizes with the same name and tenant.
        var query = _dbContext.TShirtSizes.AsNoTracking().Where(x =>x.TenantId == tenantId && !x.Deleted && x.Name == normalizedName);
        // Exclude the current record when checking during an update operation.
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        // Return true when a matching T-Shirt Size already exists.
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Create

    /// <summary>
    /// Adds a T-Shirt Size to the current DbContext.
    /// </summary>
    public Task AddAsync(TShirtSize tShirtSize, CancellationToken cancellationToken = default) => _dbContext.TShirtSizes.AddAsync(tShirtSize, cancellationToken).AsTask();

    #endregion

    #region Update
    /// <summary>
    /// Marks a T-Shirt Size as modified in the current DbContext.
    /// </summary>
    public void Update(TShirtSize tShirtSize) => _dbContext.TShirtSizes.Update(tShirtSize);
    #endregion

    #region Delete

    /// <summary>
    /// Removes a T-Shirt Size from the current DbContext.
    /// </summary>
    public void Remove(TShirtSize tShirtSize) => _dbContext.TShirtSizes.Remove(tShirtSize);

    #endregion
}