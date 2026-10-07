using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for Class Category records.
/// </summary>
internal sealed class ClassCategoryRepository : IClassCategoryRepository
{
    #region Fields
    private readonly SakoleeDbContext _dbContext;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes the Class Category repository.
    /// </summary>
    public ClassCategoryRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;
    #endregion

    #region Names
    /// <summary>
    /// Names of the given (non-deleted) categories, keyed by id.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        // Remove duplicate IDs and create a list for the query.
        var idList = ids.Distinct().ToList();
        // Return an empty dictionary when no IDs are provided.
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }
        // Get category names for the requested IDs.
        return await _dbContext.ClassCategories.Where(c => !c.Deleted && idList.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
    }
    #endregion

    #region List
    /// <summary>
    /// Gets all non-deleted Class Categories belonging to the specified tenant.
    /// Supports searching by category name or category type.
    /// </summary>
    public async Task<(IReadOnlyList<ClassCategory> Items, int Total)> ListByTenantAsync(Guid tenantId, string? name = null,string? categoryType = null,bool showDeleted = false,bool? active = null, string? search = null, string? sortBy = null,bool descending = false, int page = 1,int limit = 20, CancellationToken cancellationToken = default)
    {
        // Get records for the current tenant.
        // When showDeleted is true, bypass the global soft-delete filter.
        var query = showDeleted ? _dbContext.ClassCategories .IgnoreQueryFilters() .Where(x => x.TenantId == tenantId) : _dbContext.ClassCategories.Where(x => x.TenantId == tenantId);
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
        // Filter by Name when provided.
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            query = query.Where(x => x.Name.Contains(name));
        }
        // Filter by Category Type when provided.
        if (!string.IsNullOrWhiteSpace(categoryType))
        {
            categoryType = categoryType.Trim();
            query = query.Where(x => x.CategoryType == categoryType);
        }
        // Apply search filtering only when search text is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            // Search by either Category Name or Category Type.
            // CategoryType can be null, so check for null before using Contains().
            query = query.Where(c =>c.Name.Contains(search) || (c.CategoryType != null && c.CategoryType.Contains(search)));
        }
        // Get the total number of records before pagination.
        var total = await query.CountAsync(cancellationToken);
        // Apply sorting.
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "categorytype" => descending ? query.OrderByDescending(c => c.CategoryType) : query.OrderBy(c => c.CategoryType),
            "active" =>descending ? query.OrderByDescending(x => x.Active) : query.OrderBy(x => x.Active),
            "createdonutc" => descending ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderBy(c => c.CreatedOnUtc),
            "updatedonutc" => descending ? query.OrderByDescending(c => c.UpdatedOnUtc) : query.OrderBy(c => c.UpdatedOnUtc),
            // Default: show the most recently created or updated record first.
            _ => query.OrderByDescending(x => x.UpdatedOnUtc > x.CreatedOnUtc ? x.UpdatedOnUtc : x.CreatedOnUtc)
            //_ => descending ? query.OrderByDescending(c => c.CategoryType).ThenByDescending(c => c.Name) : query.OrderBy(c => c.CategoryType).ThenBy(c => c.Name)
        };
        // If categories are found, get the tenant information and assign it to each category.
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(cancellationToken);
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            foreach (var item in items) item.Tenant = tenant;
        }
        return (items, total);
    }
    #endregion

    #region Get 
    /// <summary>
    /// Gets a non-deleted Class Category by id for the specified tenant.
    /// </summary>
    public async Task<ClassCategory?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Get the category only if it belongs to the tenant and is not deleted.
        var category = await _dbContext.ClassCategories.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.Deleted, cancellationToken);
        // Load tenant details when the category exists.
        if (category is not null)
        {
            category.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        }
        return category;
    }
    #endregion

    #region ExistsByName
    /// <summary>
    /// Checks whether a non-deleted Class Category with the specified
    /// name already exists for the given tenant.
    /// When excludeId is provided, that record is ignored.
    /// This is required during update so that a category does not
    /// conflict with itself.
    /// </summary>
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name, string categoryType, Guid? excludeId = null,CancellationToken cancellationToken = default)
    {
        // Trim the name before checking for duplicates.
        var normalizedName = name.Trim();
        var normalizedType = categoryType.Trim();
        // Check for an existing non-deleted category with the same name.
        var query = _dbContext.ClassCategories.AsNoTracking().Where(c =>c.TenantId == tenantId && !c.Deleted && c.CategoryType == normalizedType && c.Name == normalizedName);
        // Exclude the current category when checking during an update.
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        // Return true when a matching category exists.
        return await query.AnyAsync(cancellationToken);
    }
    #endregion

    #region Create
    /// <summary>
    /// Adds a new Class Category to the database context.
    /// </summary>
    public Task AddAsync(ClassCategory classCategory,CancellationToken cancellationToken = default) => _dbContext.ClassCategories.AddAsync(classCategory, cancellationToken).AsTask();

    #endregion

    #region Update
    /// <summary>
    /// Updates an existing Class Category in the database context.
    /// </summary>
    public void Update(ClassCategory classCategory) => _dbContext.ClassCategories.Update(classCategory);

    #endregion

    #region Delete
    /// <summary>
    /// Removes the specified Class Category from the database context.
    /// </summary>
    public void Remove(ClassCategory classCategory) => _dbContext.ClassCategories.Remove(classCategory);

    #endregion
}