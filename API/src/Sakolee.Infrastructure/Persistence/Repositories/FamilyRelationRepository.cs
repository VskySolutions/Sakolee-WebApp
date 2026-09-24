using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing family relation data operations using Entity Framework Core.
/// </summary>
internal sealed class FamilyRelationRepository : IFamilyRelationRepository
{
    private readonly SakoleeDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyRelationRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public FamilyRelationRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<FamilyRelation> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FamilyRelations.AsQueryable();

        // Tenant Filter
        if (tenantId.HasValue)
        {
            query = query.Where(f => f.TenantId == tenantId.Value);
        }

        //  Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(f => f.Name.Contains(term));
        }

        //  Active Status Filter
        if (active.HasValue)
        {
            query = query.Where(f => f.Active == active.Value);
        }

        //  Total Count Before Paging
        var totalCount = await query.CountAsync(cancellationToken);

        //  Dynamic Sorting
        query = sort.SortBy?.ToLower() switch
        {
            "name" => sort.Descending ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name),
            "createdonutc" or "createdon" => sort.Descending ? query.OrderByDescending(f => f.CreatedOnUtc) : query.OrderBy(f => f.CreatedOnUtc),
            _ => sort.Descending ? query.OrderByDescending(f => f.UpdatedOnUtc) : query.OrderBy(f => f.UpdatedOnUtc)
        };

        //  Pagination (Skip & Take)
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<FamilyRelation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.FamilyRelations
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeFamilyRelationId = null,
        CancellationToken cancellationToken = default)
        => _dbContext.FamilyRelations.AnyAsync(
            f =>
                f.Name == name &&
                f.TenantId == tenantId &&
                (excludeFamilyRelationId == null ||
                 f.Id != excludeFamilyRelationId),
            cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(
        FamilyRelation familyRelation,
        CancellationToken cancellationToken = default)
        => _dbContext.FamilyRelations
            .AddAsync(familyRelation, cancellationToken)
            .AsTask();

    /// <inheritdoc />
    public void Update(FamilyRelation familyRelation)
        => _dbContext.FamilyRelations.Update(familyRelation);

    /// <inheritdoc />
    public void Remove(FamilyRelation familyRelation)
        => _dbContext.FamilyRelations.Remove(familyRelation);
}