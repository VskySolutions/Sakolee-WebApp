using Microsoft.EntityFrameworkCore;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing policy data operations using Entity Framework Core.
/// Covers the <c>Policy</c> table and its <c>PolicyClassMapping</c> join rows, from both the policy side
/// (Policies screen) and the class side (class form's Policies picker).
/// </summary>
internal sealed class PolicyRepository : IPolicyRepository
{
    #region Fields

    private readonly SakoleeDbContext _dbContext;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public PolicyRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #endregion

    #region List

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Policy> Items, int Total)> ListAsync(
        string? search,
        Guid? tenantId,
        bool? showDeleted,
        SortRequest sort,
        int page,
        int limit,
        CancellationToken cancellationToken = default)
    {
        // Query filters are bypassed so soft-deleted rows can be included on request; the tenant is
        // therefore applied explicitly here instead.
        var query = tenantId is { } tid
            ? _dbContext.Policies.IgnoreQueryFilters().Where(p => p.TenantId == tid)
            : _dbContext.Policies.IgnoreQueryFilters().AsQueryable();

        // Exclude soft-deleted policies unless they were explicitly requested.
        if (showDeleted != true)
        {
            query = query.Where(p => !p.Deleted);
        }

        // Free-text search over the policy name and description.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));
        }

        // Total count is taken before paging, for the grid's record count.
        var total = await query.CountAsync(cancellationToken);

        // Sorting by any sortable grid column; newest first by default.
        query = sort.SortBy?.ToLower() switch
        {
            "name" => sort.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "description" => sort.Descending ? query.OrderByDescending(p => p.Description) : query.OrderBy(p => p.Description),
            "active" => sort.Descending ? query.OrderByDescending(p => p.Active) : query.OrderBy(p => p.Active),
            "displayorder" => sort.Descending ? query.OrderByDescending(p => p.DisplayOrder) : query.OrderBy(p => p.DisplayOrder),
            "createdonutc" or "createdon" => sort.Descending ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc),
            "updatedonutc" or "updatedon" => sort.Descending ? query.OrderByDescending(p => p.UpdatedOnUtc) : query.OrderBy(p => p.UpdatedOnUtc),
            _ => sort.Descending ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc)
        };

        // One page of policies, with their class mappings for the grid's Classes column.
        var items = await query
            .Include(p => p.ClassMappings)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        // Tenant and soft-delete filtering come from the DbContext query filter.
        return await _dbContext.Policies
            .Where(p => p.Active)
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Policy>> ListByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        // Inactive policies are included: a class keeps showing a policy it was given until it is removed.
        return await _dbContext.Policies
            .Where(p => p.ClassMappings.Any(m => m.ClassId == classId))
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Get

    /// <inheritdoc />
    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Mappings are loaded with the policy so the caller can read and replace its classes.
        return await _dbContext.Policies
            .Include(p => p.ClassMappings)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    #endregion

    #region ExistsByName

    /// <inheritdoc />
    public async Task<bool> PolicyNameExistsAsync(
        Guid tenantId,
        string name,
        Guid? excludingPolicyId = null,
        CancellationToken cancellationToken = default)
    {
        // Soft-deleted policies are skipped by the query filter, so a deleted name can be reused.
        return await _dbContext.Policies.AnyAsync(
            p => p.TenantId == tenantId &&
                 p.Name == name &&
                 (!excludingPolicyId.HasValue || p.Id != excludingPolicyId.Value),
            cancellationToken);
    }

    #endregion

    #region Create

    /// <inheritdoc />
    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _dbContext.Policies.AddAsync(policy, cancellationToken);
    }

    #endregion

    #region Update

    /// <inheritdoc />
    public void Update(Policy policy)
    {
        // A policy loaded through this repository is already tracked; Update() would walk the graph and
        // mark newly added (composite-key) mappings as Modified rather than Added.
        if (_dbContext.Entry(policy).State == EntityState.Detached)
        {
            _dbContext.Policies.Update(policy);
        }
    }

    #endregion

    #region Class Mappings

    /// <inheritdoc />
    public void SetClasses(Policy policy, IEnumerable<Guid> classIds)
    {
        var wanted = classIds.Where(id => id != Guid.Empty).ToHashSet();

        // Remove mappings to classes that are no longer selected.
        var stale = policy.ClassMappings.Where(m => !wanted.Contains(m.ClassId)).ToList();
        foreach (var mapping in stale)
        {
            policy.ClassMappings.Remove(mapping);
            _dbContext.PolicyClassMappings.Remove(mapping);
        }

        // Add mappings for newly selected classes; explicit Add so EF inserts rather than updates them.
        var existing = policy.ClassMappings.Select(m => m.ClassId).ToHashSet();
        foreach (var classId in wanted.Where(id => !existing.Contains(id)))
        {
            var mapping = new PolicyClassMapping { PolicyId = policy.Id, ClassId = classId };
            policy.ClassMappings.Add(mapping);
            _dbContext.PolicyClassMappings.Add(mapping);
        }
    }

    /// <inheritdoc />
    public async Task SetPoliciesForClassAsync(Guid classId, IEnumerable<Guid> policyIds, CancellationToken cancellationToken = default)
    {
        var requested = policyIds.Where(id => id != Guid.Empty).ToHashSet();

        // Classes are not tenant-scoped, so only this tenant's mappings are touched: the join onto Policies
        // carries the tenant (and soft-delete) query filter.
        var current = await _dbContext.PolicyClassMappings
            .Where(m => m.ClassId == classId && _dbContext.Policies.Any(p => p.Id == m.PolicyId))
            .ToListAsync(cancellationToken);

        // Keep only ids that are live policies of this tenant; anything else is ignored.
        var wanted = await _dbContext.Policies
            .Where(p => requested.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Remove mappings to policies that are no longer selected.
        _dbContext.PolicyClassMappings.RemoveRange(current.Where(m => !wanted.Contains(m.PolicyId)));

        // Add mappings for newly selected policies.
        var existing = current.Select(m => m.PolicyId).ToHashSet();
        foreach (var policyId in wanted.Where(id => !existing.Contains(id)))
        {
            await _dbContext.PolicyClassMappings.AddAsync(new PolicyClassMapping { PolicyId = policyId, ClassId = classId }, cancellationToken);
        }
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public void Remove(Policy policy)
    {
        // Mappings are plain join rows (hard delete); the policy itself is soft-deleted by the DbContext.
        _dbContext.PolicyClassMappings.RemoveRange(policy.ClassMappings);
        _dbContext.Policies.Remove(policy);
    }

    #endregion
}
