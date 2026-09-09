using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class ActivityEventRepository : IActivityEventRepository
{
    private readonly SakoleeDbContext _dbContext;

    public ActivityEventRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(ActivityEvent activityEvent, CancellationToken cancellationToken = default)
        => _dbContext.ActivityEvents.AddAsync(activityEvent, cancellationToken).AsTask();

    public async Task<(IReadOnlyList<ActivityEvent> Items, int Total)> ListAsync(
        EntityType entityType, Guid entityId, int page, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ActivityEvents
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.CreatedOnUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}
