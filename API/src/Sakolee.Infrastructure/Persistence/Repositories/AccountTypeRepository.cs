using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for Account Type records.
/// </summary>
internal sealed class AccountTypeRepository : IAccountTypeRepository
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Account Type repository.
    /// </summary>
    public AccountTypeRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets Account Type records belonging to the specified tenant.
    /// Supports filtering, searching, sorting, and pagination.
    /// </summary>
    public async Task<(IReadOnlyList<AccountType> Items, int Total)> ListByTenantAsync(Guid tenantId,string? name = null,bool showDeleted = false,bool? active = null, string? search = null,string? sortBy = null,bool descending = false,int page = 1,int limit = 20,CancellationToken cancellationToken = default)
    {
        // Start the query using the Account Types table.
        // When showDeleted is enabled, IgnoreQueryFilters() allows
        // soft-deleted records to be included in the result.
        var query = showDeleted ? _dbContext.AccountTypes.IgnoreQueryFilters().Where(x => x.TenantId == tenantId) : _dbContext.AccountTypes.Where(x => x.TenantId == tenantId);
        // When deleted records are not requested, explicitly exclude
        // records marked as deleted.
        if (!showDeleted)
        {
            query = query.Where(x => !x.Deleted);
        }
        // Apply the Active/Inactive filter when a value is provided.
        if (active.HasValue)
        {
            query = query.Where(x => x.Active == active.Value);
        }
        // Filter by Account Type name when a name filter is provided.
        if (!string.IsNullOrWhiteSpace(name))
        {
            // Remove unnecessary spaces from the filter value.
            name = name.Trim();
            // Search Account Type names containing the search text.
            query = query.Where(x => x.Name.Contains(name));
        }
        // Apply the general search filter.
        // Currently the Account Type table only has Name as a searchable field.
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Remove unnecessary spaces from the search text.
            search = search.Trim();
            // Search Account Type names containing the search text.
            query = query.Where(x => x.Name.Contains(search));
        }
        // Get the total number of records before pagination.
        // This value is used by the frontend to calculate total pages.
        var total = await query.CountAsync(cancellationToken);
        // Apply sorting based on the requested column.
        query = sortBy?.ToLowerInvariant() switch
        {
            // Sort Account Types by Name.
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            // Sort Account Types by Active status.
            "active" => descending ? query.OrderByDescending(x => x.Active) : query.OrderBy(x => x.Active),
            // Sort Account Types by creation date.
            "createdonutc" => descending ? query.OrderByDescending(x => x.CreatedOnUtc) : query.OrderBy(x => x.CreatedOnUtc),
            // Sort Account Types by last update date.
            "updatedonutc" => descending ? query.OrderByDescending(x => x.UpdatedOnUtc) : query.OrderBy(x => x.UpdatedOnUtc),
            // Default sorting:
            // Show the most recently created or updated records first.
            _ => query.OrderByDescending(x => x.UpdatedOnUtc > x.CreatedOnUtc ? x.UpdatedOnUtc : x.CreatedOnUtc)
        };
        // Apply pagination after filtering and sorting. Skip moves to the requested page. Take limits the number of records returned.
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(cancellationToken);
        // Load the tenant information only when records exist.
        if (items.Count > 0)
        {
            // Get the tenant associated with the requested tenant ID.
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
            // Assign the tenant to every Account Type record.
            // This is used when creating the API summary response.
            foreach (var item in items)
            {
                item.Tenant = tenant;
            }
        }
        // Return both the current page records and the total record count.
        return (items, total);
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets a non-deleted Account Type record by identifier
    /// for the specified tenant.
    /// </summary>
    public async Task<AccountType?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default)
    {
        // Find the Account Type using both its ID and tenant ID.
        // Deleted records are excluded because this method is used
        // for normal view/edit operations.
        var accountType = await _dbContext.AccountTypes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.Deleted,cancellationToken);
        // If the Account Type exists, load its tenant information.
        if (accountType is not null)
        {
            accountType.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId,cancellationToken);
        }
        // Return the Account Type or null when it was not found.
        return accountType;
    }

    #endregion

    #region ExistsByName

    /// <summary>
    /// Checks whether a non-deleted Account Type with the specified
    /// name already exists for the tenant.
    /// </summary>
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default)
    {
        // Remove leading and trailing spaces before checking
        // whether the Account Type name already exists.
        var normalizedName = name.Trim();
        // Build a query for non-deleted Account Types
        // belonging to the specified tenant.
        var query = _dbContext.AccountTypes.AsNoTracking().Where(x => x.TenantId == tenantId && !x.Deleted && x.Name == normalizedName);
        // During an update, exclude the current record from the
        // duplicate check so that it can keep its existing name.
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        // Return true when another Account Type with the same
        // name already exists for this tenant.
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Create

    /// <summary>
    /// Adds an Account Type record to the current DbContext.
    /// </summary>
    public Task AddAsync(AccountType accountType,CancellationToken cancellationToken = default)
    {
        // Add the new Account Type to the DbContext.
        // The actual database insert happens when SaveChangesAsync()
        // is called by the application.
        return _dbContext.AccountTypes.AddAsync(accountType, cancellationToken).AsTask();
    }

    #endregion

    #region Update

    /// <summary>
    /// Marks an Account Type record as modified.
    /// </summary>
    public void Update(AccountType accountType) => _dbContext.AccountTypes.Update(accountType);
    #endregion

    #region Delete

    /// <summary>
    /// Removes an Account Type record from the current DbContext.
    /// </summary>
    public void Remove(AccountType accountType) => _dbContext.AccountTypes.Remove(accountType);
    #endregion
}