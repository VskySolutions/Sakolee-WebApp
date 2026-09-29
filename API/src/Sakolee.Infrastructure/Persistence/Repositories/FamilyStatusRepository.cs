using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing family status data persistence operations.
/// </summary>
internal sealed class FamilyStatusRepository : IFamilyStatusRepository
{
    #region Field Declarations

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyStatusRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public FamilyStatusRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion

    #region Read Operations

    /// <summary>
    /// Retrieves a single family status record by its unique identifier.
    /// </summary>
    public Task<FamilyStatus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.FamilyStatuses
            .FirstOrDefaultAsync(f => f.FamilyStatusId == id, cancellationToken);

    /// <summary>
    /// Performs a cross-tenant (Super Admin) read of a single family status, bypassing the ambient tenant filter.
    /// </summary>
    public Task<FamilyStatus?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.FamilyStatuses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.FamilyStatusId == id && !f.IsDeleted, cancellationToken);

    /// <summary>
    /// Retrieves a collection of family status records matching a list of identifiers.
    /// </summary>
    public async Task<IReadOnlyList<FamilyStatus>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await _dbContext.FamilyStatuses
            .Where(f => ids.Contains(f.FamilyStatusId))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Checks whether a family status with the specified name already exists (case-insensitive).
    /// </summary>
    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
        => _dbContext.FamilyStatuses.AnyAsync(f => f.Name.ToLower() == name.ToLower(), cancellationToken);

    #endregion

    #region Sorting Configuration

    // Defines allowable sort mappings for family status queries.
    private static readonly SortMap<FamilyStatus> Sorts = new SortMap<FamilyStatus>("updatedOn")
        .Add("name", f => f.Name)
        .Add("isActive", f => !f.IsDeleted, f => f.UpdatedOn)
        .Add("createdOn", f => f.CreatedOn)
        .Add("updatedOn", f => f.UpdatedOn);

    #endregion

    #region List & Filtering Operations

    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of family status records.
    /// </summary>
    public async Task<(IReadOnlyList<FamilyStatus> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, bool? isActive, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default)
    {
        // Cross-tenant (Super Admin) reads pass an explicit tenant id and bypass the ambient filter;
        // everyone else gets the ambient-filtered set, pinned to their active tenant.
        var query = tenantId is { } tid
            ? _dbContext.FamilyStatuses.IgnoreQueryFilters().Where(f => f.TenantId == tid && !f.IsDeleted)
            : _dbContext.FamilyStatuses.AsQueryable().Include(f => f.Tenant);

        // Apply search keyword filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(f => f.Name.Contains(term));
        }

        // Apply active status filter if provided
        if (isActive is { } active)
        {
            query = query.Where(f => !f.IsDeleted == active);
        }

        // Calculate total count and apply sorting/pagination
        var total = await query.CountAsync(cancellationToken);
        var items = await Sorts.Apply(query, sort.SortBy, sort.Descending)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var userIds = items
            .SelectMany(f => new[] { f.CreatedBy, f.UpdatedBy })
            .Where(id => !string.IsNullOrEmpty(id) && Guid.TryParse(id, out _))
            .Distinct()
            .Select(id => Guid.Parse(id!))
            .ToList();

        
        var usersDict = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        return (items, total);
    }



    /// <summary>
    /// Retrieves a selectable list of active family statuses for dropdown bindings.
    /// </summary>
    public async Task<IReadOnlyList<FamilyStatus>> ListSelectableAsync(
        Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Naming a tenant means reading OUTSIDE the ambient one, so the filters come off — and with them
        // the soft-delete predicate they carry, which is why `IsDeleted` is then stated in full.
        var query = tenantId is { } scope
            ? _dbContext.FamilyStatuses.IgnoreQueryFilters().Where(f => !f.IsDeleted && f.TenantId == scope)
            : _dbContext.FamilyStatuses.AsQueryable();

        return await query
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    #endregion

    /// <summary>
    /// Checks whether a family status with the specified name already exists (case-insensitive), optionally excluding a specific ID for updates.
    /// </summary>
    public Task<bool> ExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FamilyStatuses.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(f => f.FamilyStatusId != excludeId.Value);
        }
        return query.AnyAsync(f => f.Name.ToLower() == name.ToLower(), cancellationToken);
    }



    #region Command Operations (Add, Update, Remove)

    /// <summary>
    /// Adds a new family status entity to the persistence context.
    /// </summary>
    public async Task AddAsync(FamilyStatus familyStatus, CancellationToken cancellationToken = default)
        => await _dbContext.FamilyStatuses.AddAsync(familyStatus, cancellationToken);

    /// <summary>
    /// Marks an existing family status entity as modified.
    /// </summary>
    public void Update(FamilyStatus familyStatus)
        => _dbContext.FamilyStatuses.Update(familyStatus);

    /// <summary>
    /// Marks an existing family status entity for removal.
    /// </summary>
    public void Remove(FamilyStatus familyStatus)
        => _dbContext.FamilyStatuses.Remove(familyStatus);

    #endregion
}