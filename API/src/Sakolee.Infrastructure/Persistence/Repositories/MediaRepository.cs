using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class MediaRepository : IMediaRepository
{
    private readonly SakoleeDbContext _dbContext;

    public MediaRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Media?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Media.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task AddAsync(Media media, CancellationToken cancellationToken = default)
        => await _dbContext.Media.AddAsync(media, cancellationToken);

    public void Update(Media media) => _dbContext.Media.Update(media);
}
