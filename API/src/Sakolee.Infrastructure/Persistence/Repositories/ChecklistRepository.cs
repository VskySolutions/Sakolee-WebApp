using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class ChecklistRepository : IChecklistRepository
{
    private readonly SakoleeDbContext _dbContext;

    public ChecklistRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(Checklist checklist, CancellationToken cancellationToken = default)
        => _dbContext.Checklists.AddAsync(checklist, cancellationToken).AsTask();

    public Task<Checklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Checklists
            .Include(c => c.Items.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Remove(Checklist checklist) => _dbContext.Checklists.Remove(checklist);

    public async Task<IReadOnlyList<Checklist>> ListAsync(EntityType entityType, Guid entityId, CancellationToken cancellationToken = default)
        => await _dbContext.Checklists
            .Include(c => c.Items.OrderBy(i => i.SortOrder))
            .Where(c => c.EntityType == entityType && c.EntityId == entityId)
            .OrderBy(c => c.CreatedOnUtc)
            .ToListAsync(cancellationToken);

    public Task AddItemAsync(ChecklistItem item, CancellationToken cancellationToken = default)
        => _dbContext.ChecklistItems.AddAsync(item, cancellationToken).AsTask();

    public Task<ChecklistItem?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default)
        => _dbContext.ChecklistItems.FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

    public void UpdateItem(ChecklistItem item) => _dbContext.ChecklistItems.Update(item);

    public void RemoveItem(ChecklistItem item) => _dbContext.ChecklistItems.Remove(item);
}
