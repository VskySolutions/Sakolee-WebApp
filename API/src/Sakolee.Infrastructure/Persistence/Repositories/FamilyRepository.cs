using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class FamilyRepository : IFamilyRepository
{
    private readonly SakoleeDbContext _dbContext;

    public FamilyRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    private static readonly SortMap<Family> Sorts = new SortMap<Family>("updatedOnUtc")
        .Add("familyName", f => f.FamilyName)
        .Add("createdOnUtc", f => f.CreatedOnUtc)
        .Add("updatedOnUtc", f => f.UpdatedOnUtc);

    public Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Families
            .Include(f => f.Contacts.Where(c => !c.Deleted))
            .Include(f => f.FamilyStatus)
            .Include(f => f.StudioLocation)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Family?> GetByIdUnscopedAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Families
            .IgnoreQueryFilters()
            .Include(f => f.Contacts.Where(c => !c.Deleted))
            .Include(f => f.FamilyStatus)
            .Include(f => f.StudioLocation)
            .FirstOrDefaultAsync(f => f.Id == id && !f.Deleted, cancellationToken);

    public Task<bool> FamilyNameExistsAsync(Guid tenantId, string familyName, Guid? excludingFamilyId = null, CancellationToken cancellationToken = default)
        => _dbContext.Families.AnyAsync(
            f => f.TenantId == tenantId && f.FamilyName != null && f.FamilyName.ToLower() == familyName.ToLower()
                && (!excludingFamilyId.HasValue || f.Id != excludingFamilyId.Value),
            cancellationToken);

    public async Task<(IReadOnlyList<Family> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, Guid? familyStatusId, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default)
    {
        // Cross-tenant (Super Admin) reads pass an explicit tenant id and bypass the ambient filter;
        // everyone else gets the ambient-filtered set, pinned to their active tenant.
        var query = tenantId is { } tid
            ? _dbContext.Families.IgnoreQueryFilters().Where(f => f.TenantId == tid && !f.Deleted)
            : _dbContext.Families.AsQueryable();

        query = query
            .Include(f => f.Contacts.Where(c => !c.Deleted))
            .Include(f => f.FamilyStatus)
            .Include(f => f.StudioLocation);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(f => f.FamilyName != null && f.FamilyName.Contains(term));
        }

        if (familyStatusId is { } statusId)
        {
            query = query.Where(f => f.FamilyStatusId == statusId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await Sorts.Apply(query, sort.SortBy, sort.Descending)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Family family, CancellationToken cancellationToken = default)
        => await _dbContext.Families.AddAsync(family, cancellationToken);

    public void Update(Family family) => _dbContext.Families.Update(family);

    public void Remove(Family family) => _dbContext.Families.Remove(family);

    public async Task<IReadOnlyList<FamilyPersonMapping>> ListContactsAsync(Guid familyId, CancellationToken cancellationToken = default)
        => await _dbContext.FamilyPersonMappings.Where(c => c.FamilyId == familyId && !c.Deleted).ToListAsync(cancellationToken);

    public async Task AddContactAsync(FamilyPersonMapping contact, CancellationToken cancellationToken = default)
        => await _dbContext.FamilyPersonMappings.AddAsync(contact, cancellationToken);

    public void UpdateContact(FamilyPersonMapping contact) => _dbContext.FamilyPersonMappings.Update(contact);
}
