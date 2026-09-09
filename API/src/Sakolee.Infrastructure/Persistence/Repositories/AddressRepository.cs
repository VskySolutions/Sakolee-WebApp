using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class AddressRepository : IAddressRepository
{
    private readonly SakoleeDbContext _dbContext;

    public AddressRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Address address, CancellationToken cancellationToken = default)
        => await _dbContext.Addresses.AddAsync(address, cancellationToken);

    public void Update(Address address) => _dbContext.Addresses.Update(address);
}
