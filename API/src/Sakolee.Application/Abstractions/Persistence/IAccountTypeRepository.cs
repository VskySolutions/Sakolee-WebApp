using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Provides data access operations for Account Type records.
/// </summary>
public interface IAccountTypeRepository
{
    /// <summary>
    /// Gets Account Type records belonging to the given tenant.
    /// Supports filtering, searching, sorting, and pagination.
    /// </summary>
    Task<(IReadOnlyList<AccountType> Items, int Total)> ListByTenantAsync(Guid tenantId,string? name = null,bool showDeleted = false,bool? active = null,string? search = null,string? sortBy = null,bool descending = false,int page = 1,int limit = 20,CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a non-deleted Account Type record by identifier
    /// for the specified tenant.
    /// </summary>
    Task<AccountType?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a non-deleted Account Type with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an Account Type record to the current DbContext.
    /// </summary>
    Task AddAsync(AccountType accountType,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an Account Type record as modified.
    /// </summary>
    void Update(AccountType accountType);

    /// <summary>
    /// Removes an Account Type record from the current DbContext.
    /// </summary>
    void Remove(AccountType accountType);
}