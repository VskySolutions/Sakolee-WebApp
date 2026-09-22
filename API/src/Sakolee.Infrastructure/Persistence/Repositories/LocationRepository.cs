using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class LocationRepository : ILocationRepository
{
    private readonly SakoleeDbContext _dbContext;

    public LocationRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Location>> ListAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.Locations
            .OrderByDescending(l => l.UpdatedOnUtc)
            .ToListAsync(cancellationToken);

    public Task<Location?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.Locations
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeLocationId = null,
        CancellationToken cancellationToken = default)
        => _dbContext.Locations.AnyAsync(
            l =>
                l.Name == name &&
                l.TenantId == tenantId &&
                (excludeLocationId == null ||
                 l.Id != excludeLocationId),
            cancellationToken);

    public Task AddAsync(
        Location location,
        CancellationToken cancellationToken = default)
        => _dbContext.Locations
            .AddAsync(location, cancellationToken)
            .AsTask();

    public void Update(Location location)
        => _dbContext.Locations.Update(location);

    public void Remove(Location location)
        => _dbContext.Locations.Remove(location);
}