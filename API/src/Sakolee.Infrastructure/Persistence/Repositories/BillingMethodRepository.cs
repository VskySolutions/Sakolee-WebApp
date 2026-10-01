using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing bank method data operations using Entity Framework Core.
/// </summary>
internal sealed class BillingMethodRepository : IBillingMethodRepository
{
    private readonly SakoleeDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillingMethodRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public BillingMethodRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<BillingMethod> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         bool? showDeleted,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default)
    {
        //var query = _dbContext.BillingMethod.AsQueryable();

        var query = tenantId is { } tid
          ? _dbContext.BillingMethod.IgnoreQueryFilters().Where(s => s.TenantId == tid)
          : _dbContext.BillingMethod.IgnoreQueryFilters().AsQueryable();
        if (showDeleted != true)
        {
            query = query.Where(s => !s.Deleted);
        }

        //  Tenant Filter
        if (tenantId.HasValue)
        {
            query = query.Where(b => b.TenantId == tenantId.Value);
        }

        //  Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => b.Name.Contains(term));
        }

        //  Active Status Filter
        if (active.HasValue)
        {
            query = query.Where(b => b.Active == active.Value);
        }

        //  Total Count Before Paging
        var totalCount = await query.CountAsync(cancellationToken);

        //  Dynamic Sorting
        query = sort.SortBy?.ToLower() switch
        {
            "name" => sort.Descending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "createdonutc" or "createdon" => sort.Descending ? query.OrderByDescending(b => b.CreatedOnUtc) : query.OrderBy(b => b.CreatedOnUtc),
            _ => sort.Descending ? query.OrderByDescending(b => b.UpdatedOnUtc) : query.OrderBy(b => b.UpdatedOnUtc)
        };

        //  Pagination (Skip & Take)
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<BillingMethod?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.BillingMethod
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeBankMethodId = null,
        CancellationToken cancellationToken = default)
        => _dbContext.BillingMethod.AnyAsync(
            b =>
                b.Name == name &&
                b.TenantId == tenantId &&
                (excludeBankMethodId == null ||
                 b.Id != excludeBankMethodId),
            cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(
        BillingMethod bankMethod,
        CancellationToken cancellationToken = default)
        => _dbContext.BillingMethod
            .AddAsync(bankMethod, cancellationToken)
            .AsTask();

    /// <inheritdoc />
    public void Update(BillingMethod bankMethod)
        => _dbContext.BillingMethod.Update(bankMethod);

    /// <inheritdoc />
    public void Remove(BillingMethod bankMethod)
        => _dbContext.BillingMethod.Remove(bankMethod);
}