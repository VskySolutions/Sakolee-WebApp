using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class StudentRepository : IStudentRepository
{
    private readonly SakoleeDbContext _dbContext;

    public StudentRepository(SakoleeDbContext dbContext) => _dbContext = dbContext;

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> IsOwnedByTenantAsync(Guid? personId, Guid tenantId, CancellationToken cancellationToken = default)
        => personId.HasValue && await _dbContext.TenantPersonMappings.AnyAsync(
            m => !m.Deleted && m.PersonId == personId.Value && m.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<Student>> ListAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        // Read whole and filtered in memory (TenantsController does the same for Tenants): PersonId is
        // stored as nvarchar rather than uniqueidentifier on this table, so a SQL-side join against
        // TenantPersonMapping's native Guid column would need a cross-type comparison; resolving the
        // tenant's person ids first and filtering client-side sidesteps that entirely.
        var all = await _dbContext.Students.OrderByDescending(s => s.UpdatedOnUtc ?? s.CreatedOnUtc).ToListAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return all;
        }

        var personIds = await _dbContext.TenantPersonMappings
            .Where(m => !m.Deleted && m.TenantId == tenantId.Value)
            .Select(m => m.PersonId)
            .ToListAsync(cancellationToken);
        var idSet = new HashSet<Guid>(personIds);
        return all.Where(s => s.PersonId.HasValue && idSet.Contains(s.PersonId.Value)).ToList();
    }

    public async Task<IReadOnlyList<Student>> ListByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .Where(s => !s.Deleted && s.FamilyId == familyId)
            .OrderByDescending(s => s.UpdatedOnUtc ?? s.CreatedOnUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> CountByFamilyIdsAsync(IEnumerable<Guid> familyIds, CancellationToken cancellationToken = default)
    {
        var idSet = new HashSet<Guid>(familyIds.Distinct());
        if (idSet.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        // Grouped client-side, like ListAsync's tenant filter above: FamilyId is nvarchar(450) on this
        // legacy-shaped table, and this repository's established pattern is to sidestep any SQL-side
        // translation surprises on that column by filtering in memory rather than pushing the predicate
        // down.
        var all = await _dbContext.Students.Where(s => !s.Deleted && s.FamilyId.HasValue).ToListAsync(cancellationToken);
        return all
            .Where(s => idSet.Contains(s.FamilyId!.Value))
            .GroupBy(s => s.FamilyId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
        => await _dbContext.Students.AddAsync(student, cancellationToken);

    public void Update(Student student) => _dbContext.Students.Update(student);
}
