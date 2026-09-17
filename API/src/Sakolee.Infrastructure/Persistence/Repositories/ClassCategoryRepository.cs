using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class ClassCategoryRepository : IClassCategoryRepository
{
    private readonly SakoleeDbContext _dbContext;

    public ClassCategoryRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ClassCategory>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _dbContext.ClassCategories
            .Where(c => !c.Deleted && c.TenantId == tenantId)
            .OrderBy(c => c.CategoryType).ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
}
