using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing class session data persistence operations.
/// </summary>
internal sealed class ClassSessionRepository : IClassSessionRepository
{
    #region Field Declarations

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassSessionRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public ClassSessionRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    #endregion

    #region Read Operations

    /// <summary>
    /// Retrieves a single class session record by its unique identifier.
    /// </summary>
    public Task<ClassSessions?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.ClassSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    /// <summary>
    /// Performs a cross-tenant read of a single class session, bypassing ambient query filters.
    /// </summary>
    public Task<ClassSessions?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.ClassSessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    /// <summary>
    /// Retrieves a collection of class session records matching the specified identifiers.
    /// </summary>
    public async Task<IReadOnlyList<ClassSessions>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await _dbContext.ClassSessions
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Checks whether a class session with the specified name already exists globally.
    /// </summary>
    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
        => _dbContext.ClassSessions.AnyAsync(s => s.Name.ToLower() == name.ToLower(), cancellationToken);

    /// <summary>
    /// Checks whether a class session with the specified name already exists within the tenant context, optionally excluding a specific ID.
    /// </summary>
    public Task<bool> NameExistsAsync(string name, Guid tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClassSessions
            .Where(s => s.TenantId == tenantId && s.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Sorting Configuration

    // Defines allowable sort mappings for class session queries.
    private static readonly SortMap<ClassSessions> Sorts = new SortMap<ClassSessions>("updatedOn")
        .Add("name", s => s.Name)
        .Add("active", s => s.Active)
        .Add("createdOn", s => s.CreatedOn)
        .Add("updatedOn", s => s.UpdatedOn);

    #endregion

    #region List & Filtering Operations

    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of class session records.
    /// </summary>
    public async Task<(IReadOnlyList<ClassSessions> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, bool? isActive, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default)
    {
        var query = tenantId is { } tid
            ? _dbContext.ClassSessions.IgnoreQueryFilters().Where(s => s.TenantId == tid)
            : _dbContext.ClassSessions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Name.Contains(term));
        }

        if (isActive is { } active)
        {
            query = query.Where(s => s.Active == active);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await Sorts.Apply(query, sort.SortBy, sort.Descending)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// Retrieves a selectable list of active class sessions for dropdown bindings.
    /// </summary>
    public async Task<IReadOnlyList<ClassSessions>> ListSelectableAsync(
        Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var query = tenantId is { } scope
            ? _dbContext.ClassSessions.IgnoreQueryFilters().Where(s => s.Active && s.TenantId == scope)
            : _dbContext.ClassSessions.Where(s => s.Active);

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Command Operations (Add, Update, Remove)

    /// <summary>
    /// Adds a new class session entity to the persistence context.
    /// </summary>
    public async Task AddAsync(ClassSessions session, CancellationToken cancellationToken = default)
        => await _dbContext.ClassSessions.AddAsync(session, cancellationToken);

    /// <summary>
    /// Marks an existing class session entity as modified.
    /// </summary>
    public void Update(ClassSessions session)
        => _dbContext.ClassSessions.Update(session);

    /// <summary>
    /// Marks an existing class session entity for removal.
    /// </summary>
    public void Remove(ClassSessions session)
        => _dbContext.ClassSessions.Remove(session);

    #endregion
}