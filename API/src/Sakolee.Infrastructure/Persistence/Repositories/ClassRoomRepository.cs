using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing class room data operations using Entity Framework Core.
/// </summary>
internal sealed class ClassRoomRepository : IClassRoomRepository
{
    private readonly SakoleeDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassRoomRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public ClassRoomRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<ClassRooms?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClassRooms
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ClassRooms?> GetByIdUnscopedAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClassRooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    /// <inheritdoc />
    public async Task<bool> ClassRoomNameExistsAsync(
        Guid tenantId,
        Guid locationId,
        string name,
        Guid? excludingClassRoomId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClassRooms.AnyAsync(
            c => c.TenantId == tenantId &&
            c.LocationId == locationId &&
                 c.Name == name &&
                 (!excludingClassRoomId.HasValue || c.Id != excludingClassRoomId.Value),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<ClassRooms> Items, int Total)> ListAsync(
        string? search,
        Guid? tenantId,
        Guid? locationId,
        bool? showDeleted,
        SortRequest sort,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        //var query = _dbContext.ClassRooms.AsQueryable();
        var query = tenantId is { } tid
         ? _dbContext.ClassRooms.IgnoreQueryFilters().Where(s => s.TenantId == tid)
         : _dbContext.ClassRooms.IgnoreQueryFilters().AsQueryable();
        if (showDeleted != true)
        {
            query = query.Where(s => !s.Deleted);
        }

        // Tenant Filter
        if (tenantId.HasValue)
        {
            query = query.Where(c => c.TenantId == tenantId.Value);
        }

        // Location Filter
        if (locationId.HasValue)
        {
            query = query.Where(c => c.LocationId == locationId.Value);
        }

        // Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        // Total Count Before Paging
        var total = await query.CountAsync(cancellationToken);

        // Dynamic Sorting for Every Column
        query = sort.SortBy?.ToLower() switch
        {
            "name" => sort.Descending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "active" => sort.Descending ? query.OrderByDescending(c => c.Active) : query.OrderBy(c => c.Active),
            "locationid" => sort.Descending ? query.OrderByDescending(c => c.LocationId) : query.OrderBy(c => c.LocationId),
            "tenantid" => sort.Descending ? query.OrderByDescending(c => c.TenantId) : query.OrderBy(c => c.TenantId),
            "id" => sort.Descending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
            "createdonutc" or "createdon" => sort.Descending ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderBy(c => c.CreatedOnUtc),
            "updatedonutc" or "updatedon" => sort.Descending ? query.OrderByDescending(c => c.UpdatedOnUtc) : query.OrderBy(c => c.UpdatedOnUtc),
            _ => sort.Descending ? query.OrderByDescending(c => c.UpdatedOnUtc) : query.OrderBy(c => c.UpdatedOnUtc)
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        ClassRooms classRoom,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ClassRooms.AddAsync(classRoom, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ClassRooms classRoom)
    {
        _dbContext.ClassRooms.Update(classRoom);
    }

    /// <inheritdoc />
    public void Remove(ClassRooms classRoom)
    {
        _dbContext.ClassRooms.Remove(classRoom);
    }
}