using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class ClassRepository : IClassRepository
{
    private readonly SakoleeDbContext _dbContext;

    public ClassRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Classes.FirstOrDefaultAsync(c => c.Id == id && !c.Deleted, cancellationToken);

    public async Task<IReadOnlyList<Class>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Classes
            .Where(c => !c.Deleted)
            .OrderByDescending(c => c.UpdatedOnUtc ?? c.CreatedOnUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Class @class, CancellationToken cancellationToken = default)
        => await _dbContext.Classes.AddAsync(@class, cancellationToken);

    public void Update(Class @class) => _dbContext.Classes.Update(@class);
}
