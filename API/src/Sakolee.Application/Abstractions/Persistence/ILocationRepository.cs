using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

public interface ILocationRepository
{
    Task<IReadOnlyList<Location>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<Location?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeLocationId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Location location,
        CancellationToken cancellationToken = default);

    void Update(Location location);

    void Remove(Location location);
}