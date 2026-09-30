using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for managing MembershipType data operations.
/// </summary>
public interface IMembershipTypeRepository
{
    /// <summary>
    /// Retrieves a read-only list of all membership types asynchronously.
    /// </summary>
    /// <param name="search">The search term filter.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="active">Optional active status filter.</param>
    /// <param name="showDeleted">Flag indicating whether to include deleted records.</param>
    /// <param name="sort">Sorting parameters.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="limit">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of membership types and total count.</returns>
    Task<(IReadOnlyList<MembershipType> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         bool? showDeleted,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific membership type by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the membership type.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The membership type if found; otherwise, null.</returns>
    Task<MembershipType?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a membership type with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the membership type to check.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="excludeMembershipTypeId">Optional membership type ID to exclude from the check (useful during updates).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the name exists; otherwise, false.</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeMembershipTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new membership type to the repository context.
    /// </summary>
    /// <param name="membershipType">The membership type entity to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddAsync(
        MembershipType membershipType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing membership type as updated in the repository context.
    /// </summary>
    /// <param name="membershipType">The membership type entity to update.</param>
    void Update(MembershipType membershipType);

    /// <summary>
    /// Marks an existing membership type for removal from the repository context.
    /// </summary>
    /// <param name="membershipType">The membership type entity to remove.</param>
    void Remove(MembershipType membershipType);
}