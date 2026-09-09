using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class UserDepartmentRepository : IUserDepartmentRepository
{
    private readonly SakoleeDbContext _dbContext;

    public UserDepartmentRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserDepartment?> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.UserDepartments.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

    public Task<UserDepartment?> GetHeadAsync(string department, CancellationToken cancellationToken = default)
        => _dbContext.UserDepartments
            .FirstOrDefaultAsync(d => d.IsHead && d.Department == department, cancellationToken);

    public async Task<IReadOnlyList<UserDepartment>> ListHeadsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.UserDepartments
            .Where(d => d.IsHead)
            .OrderBy(d => d.Department)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserDepartment>> ListForUsersAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<UserDepartment>();
        }

        return await _dbContext.UserDepartments
            .Where(d => ids.Contains(d.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserDepartment department, CancellationToken cancellationToken = default)
        => await _dbContext.UserDepartments.AddAsync(department, cancellationToken);

    public void Update(UserDepartment department) => _dbContext.UserDepartments.Update(department);

    public void Remove(UserDepartment department) => _dbContext.UserDepartments.Remove(department);
}
