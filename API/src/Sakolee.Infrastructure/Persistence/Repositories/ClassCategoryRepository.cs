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

    #region List 
    /// <summary>
    /// Gets all non-deleted Class Categories belonging to the specified tenant.
    /// Supports searching by category name or category type.
    /// </summary>
    public async Task<IReadOnlyList<ClassCategory>> ListByTenantAsync(Guid tenantId, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClassCategories.Where(c => !c.Deleted && c.TenantId == tenantId);
        // Apply search filtering only when search text is provided.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            // Search by either Category Name or Category Type.
            // CategoryType can be null, so check for null before using Contains().
            query = query.Where(c =>c.Name.Contains(search) || (c.CategoryType != null && c.CategoryType.Contains(search)));
        }
        // If categories are found, get the tenant information
        // and assign it to each category.
        var items = await query.OrderBy(c => c.CategoryType).ThenBy(c => c.Name).ToListAsync(cancellationToken);
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            foreach (var item in items) item.Tenant = tenant;
        }
        return items;
    }
    #endregion

    #region Get 
    /// <summary>
    /// Gets a non-deleted Class Category by id for the specified tenant.
    /// </summary>
    public async Task<ClassCategory?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.ClassCategories.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.Deleted, cancellationToken);
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
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var query = _dbContext.ClassCategories.AsNoTracking().Where(c =>c.TenantId == tenantId && !c.Deleted && c.Name == normalizedName);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
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