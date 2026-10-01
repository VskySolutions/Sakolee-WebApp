using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides database access for E-Payment Schedule records.
/// </summary>
internal sealed class EPaymentScheduleRepository : IEPaymentScheduleRepository
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the E-Payment Schedule repository.
    /// </summary>
    public EPaymentScheduleRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #endregion

    #region List

    /// <summary>
    /// Gets all non-deleted E-Payment Schedule records belonging to the specified tenant.
    /// Supports searching and sorting.
    /// </summary>
    public async Task<(IReadOnlyList<EPaymentSchedule> Items, int Total)> ListByTenantAsync(Guid tenantId,string? name = null,bool showDeleted = false,bool? active = null,string? search = null,string? sortBy = null,bool descending = false,int page = 1,int limit = 20, CancellationToken cancellationToken = default)
    {
        // When deleted records are requested, ignore the global query filter
        // so that both deleted and non-deleted records can be returned.
        var query = showDeleted ? _dbContext.EPaymentSchedules.IgnoreQueryFilters().Where(x => x.TenantId == tenantId) : _dbContext.EPaymentSchedules.Where(x => x.TenantId == tenantId);
        // When deleted records are not requested, explicitly exclude
        // records that have been soft-deleted.
        if (!showDeleted)
        {
            query = query.Where(x => !x.Deleted);
        }
        // Apply the Active filter when an Active value has been provided.
        if (active.HasValue)
        {
            query = query.Where(x => x.Active == active.Value);
        }
        // Apply the Name column filter when a name value is provided.
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            query = query.Where(x => x.Name.Contains(name));
        }
        // Apply the general search filter against the Name field.
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search));
        }
        // Get the total number of records before pagination is applied.
        var total = await query.CountAsync(cancellationToken);
        // Apply sorting based on the requested column and direction.
        // If no valid sort column is provided, the most recently
        // created or updated records are shown first.
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" =>descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "active" =>descending ? query.OrderByDescending(x => x.Active) : query.OrderBy(x => x.Active),
            "createdonutc" =>descending ? query.OrderByDescending(x => x.CreatedOnUtc) : query.OrderBy(x => x.CreatedOnUtc),
            "updatedonutc" =>descending ? query.OrderByDescending(x => x.UpdatedOnUtc) : query.OrderBy(x => x.UpdatedOnUtc),
            _ => query.OrderByDescending(x => x.UpdatedOnUtc > x.CreatedOnUtc ? x.UpdatedOnUtc : x.CreatedOnUtc)
        };
        // Apply pagination and retrieve only the records
        // required for the current page.
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(cancellationToken);
        // Load the tenant once and assign it to each returned record.
        // This allows the API model to access the tenant information.
        if (items.Count > 0)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId,cancellationToken);
            foreach (var item in items)
            {
                item.Tenant = tenant;
            }
        }
        // Return the paginated records together with the total count.
        return (items, total);
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets a non-deleted E-Payment Schedule record by identifier
    /// for the specified tenant.
    /// </summary>
    public async Task<EPaymentSchedule?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default)
    {
        // Find the E-Payment Schedule record using both its ID
        // and tenant ID to ensure tenant-level isolation.
        // Deleted records are excluded from this operation.
        var ePaymentSchedule =await _dbContext.EPaymentSchedules.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.Deleted,cancellationToken);
        // If the record exists, load its tenant information.
        if (ePaymentSchedule is not null)
        {
            ePaymentSchedule.Tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId,cancellationToken);
        }
        // Return the record when found; otherwise return null.
        return ePaymentSchedule;
    }

    #endregion

    #region ExistsByName

    /// <summary>
    /// Checks whether a non-deleted E-Payment Schedule record with the specified
    /// name already exists for the given tenant.
    /// When excludeId is provided, that record is ignored.
    /// </summary>
    public async Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default)
    {
        // Trim the provided name before checking for duplicates.
        var normalizedName = name.Trim();
        // Start the query with records belonging to the specified tenant
        // and exclude soft-deleted records.
        var query = _dbContext.EPaymentSchedules.AsNoTracking().Where(x =>x.TenantId == tenantId && !x.Deleted && x.Name == normalizedName);
        // During an update, exclude the current record from the
        // duplicate-name check.
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        // Return true when another record with the same name exists.
        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Create

    /// <summary>
    /// Adds an E-Payment Schedule record to the current DbContext.
    /// </summary>
    public Task AddAsync(EPaymentSchedule ePaymentSchedule,CancellationToken cancellationToken = default)
    {
        // Add the new E-Payment Schedule entity to the DbContext.
        // The actual database insert occurs when SaveChangesAsync is called.
        return _dbContext.EPaymentSchedules.AddAsync(ePaymentSchedule,cancellationToken).AsTask();
    }

    #endregion

    #region Update

    /// <summary>
    /// Marks an E-Payment Schedule record as modified.
    /// </summary>
    public void Update(EPaymentSchedule ePaymentSchedule) =>_dbContext.EPaymentSchedules.Update(ePaymentSchedule);

    #endregion

    #region Delete

    /// <summary>
    /// Removes an E-Payment Schedule record from the current DbContext.
    /// The DbContext converts this operation into a soft delete.
    /// </summary>
    public void Remove(EPaymentSchedule ePaymentSchedule) =>_dbContext.EPaymentSchedules.Remove(ePaymentSchedule);

    #endregion
}

