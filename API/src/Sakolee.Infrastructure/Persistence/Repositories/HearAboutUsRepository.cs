using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for Hear About Us records.
/// </summary>
internal sealed class HearAboutUsRepository : IHearAboutUsRepository
{
    #region Fields
    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Hear About Us repository.
    /// </summary>
    public HearAboutUsRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets all non-deleted Hear About Us records belonging to the specified tenant.
    /// Supports searching and sorting.
    /// </summary>
    public async Task<(IReadOnlyList<HearAboutUs> Items, int Total)> ListByTenantAsync(Guid tenantId, string? name = null,bool showDeleted = false, bool? active = null, string? search = null,string? sortBy = null,bool descending = false, int page = 1,int limit = 20, CancellationToken cancellationToken = default)
    {
        // Get non-deleted records for the current tenant.
        //var query = _dbContext.HearAboutUs.Where(x => !x.Deleted && x.TenantId == tenantId);

        // Get records for the current tenant.
        // When showDeleted is true, bypass the global soft-delete filter.
        var query = showDeleted ? _dbContext.HearAboutUs .IgnoreQueryFilters() .Where(x => x.TenantId == tenantId) : _dbContext.HearAboutUs .Where(x => x.TenantId == tenantId);
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
            //// Sort by name when no valid column is provided.
            //_ => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
            // Default: show the most recently created or updated record first.
            _ => query.OrderByDescending(x =>x.UpdatedOnUtc > x.CreatedOnUtc ? x.UpdatedOnUtc : x.CreatedOnUtc)
        };
        // Execute the query and get the records.
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(cancellationToken);
        // Load tenant details for the returned records.
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync( x => x.Id == tenantId, cancellationToken);
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
    /// Gets a non-deleted Hear About Us record by identifier
    /// for the specified tenant.
    /// </summary>
    public async Task<HearAboutUs?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default)
    {
        // Get the record only if it belongs to the tenant and is not deleted.
        var hearAboutUs = await _dbContext.HearAboutUs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.Deleted, cancellationToken);
        // Load tenant details when the record exists.
        if (hearAboutUs is not null)
        {
            hearAboutUs.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync( x => x.Id == tenantId, cancellationToken);
        }
        return hearAboutUs;
    }

    #endregion

    #region ExistsByName

    /// <summary>
    /// Checks whether a non-deleted Hear About Us record with the specified
    /// name already exists for the given tenant.
    /// When excludeId is provided, that record is ignored.
    /// </summary>
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        // Trim the name before checking for duplicates.
        var normalizedName = name.Trim();
        // Check for an existing non-deleted record with the same name.
        var query = _dbContext.HearAboutUs.AsNoTracking().Where(x =>x.TenantId == tenantId && !x.Deleted && x.Name == normalizedName);
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
    /// Adds a Hear About Us record to the current DbContext.
    /// </summary>
    public Task AddAsync(HearAboutUs hearAboutUs, CancellationToken cancellationToken = default)
    {
        return _dbContext.HearAboutUs.AddAsync(hearAboutUs, cancellationToken).AsTask();
    }

    #endregion

    #region Update

    /// <summary>
    /// Marks a Hear About Us record as modified.
    /// </summary>
    public void Update(HearAboutUs hearAboutUs) => _dbContext.HearAboutUs.Update(hearAboutUs);
    #endregion

    #region Delete

    /// <summary>
    /// Removes a Hear About Us record from the current DbContext.
    /// The DbContext converts this operation into a soft delete.
    /// </summary>
    public void Remove(HearAboutUs hearAboutUs) => _dbContext.HearAboutUs.Remove(hearAboutUs);
    
    #endregion
}