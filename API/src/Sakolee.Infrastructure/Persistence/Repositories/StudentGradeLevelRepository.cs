using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing student grade level data operations using Entity Framework Core.
/// </summary>
internal sealed class StudentGradeLevelRepository : IStudentGradeLevelRepository
{
    private readonly SakoleeDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentGradeLevelRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public StudentGradeLevelRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<StudentGradeLevel> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         bool? showDeleted,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default)
    {
        var query = tenantId is { } tid
          ? _dbContext.StudentGradeLevel.IgnoreQueryFilters().Where(s => s.TenantId == tid)
          : _dbContext.StudentGradeLevel.IgnoreQueryFilters().AsQueryable();

        if (showDeleted != true)
        {
            query = query.Where(s => !s.Deleted);
        }

        // Tenant Filter
        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        // Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Name.Contains(term));
        }

        // Active Status Filter
        if (active.HasValue)
        {
            query = query.Where(s => s.Active == active.Value);
        }

        // Total Count Before Paging
        var totalCount = await query.CountAsync(cancellationToken);

        // Dynamic Sorting
        query = sort.SortBy?.ToLower() switch
        {
            "name" => sort.Descending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "createdonutc" or "createdon" => sort.Descending ? query.OrderByDescending(s => s.CreatedOnUtc) : query.OrderBy(s => s.CreatedOnUtc),
            _ => sort.Descending ? query.OrderByDescending(s => s.UpdatedOnUtc) : query.OrderBy(s => s.UpdatedOnUtc)
        };

        // Pagination (Skip & Take)
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<StudentGradeLevel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => _dbContext.StudentGradeLevel
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeStudentGradeLevelId = null,
        CancellationToken cancellationToken = default)
        => _dbContext.StudentGradeLevel.AnyAsync(
            s =>
                s.Name == name &&
                s.TenantId == tenantId &&
                (excludeStudentGradeLevelId == null ||
                 s.Id != excludeStudentGradeLevelId),
            cancellationToken);

    /// <inheritdoc />
    public Task AddAsync(
        StudentGradeLevel studentGradeLevel,
        CancellationToken cancellationToken = default)
        => _dbContext.StudentGradeLevel
            .AddAsync(studentGradeLevel, cancellationToken)
            .AsTask();

    /// <inheritdoc />
    public void Update(StudentGradeLevel studentGradeLevel)
        => _dbContext.StudentGradeLevel.Update(studentGradeLevel);

    /// <inheritdoc />
    public void Remove(StudentGradeLevel studentGradeLevel)
        => _dbContext.StudentGradeLevel.Remove(studentGradeLevel);
}