using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly SakoleeDbContext _dbContext;

    public TenantRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        => _dbContext.Tenants.FirstOrDefaultAsync(t => t.Identifier == identifier, cancellationToken);

    public Task<bool> IdentifierExistsAsync(string identifier, CancellationToken cancellationToken = default)
        => _dbContext.Tenants.AnyAsync(t => t.Identifier == identifier, cancellationToken);

    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Tenants.OrderByDescending(t => t.UpdatedOnUtc).ToListAsync(cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => await _dbContext.Tenants.AddAsync(tenant, cancellationToken);

    public void Update(Tenant tenant)
        => _dbContext.Tenants.Update(tenant);
}
