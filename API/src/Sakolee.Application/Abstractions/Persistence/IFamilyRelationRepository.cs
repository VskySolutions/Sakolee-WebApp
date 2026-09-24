using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for managing FamilyRelation data operations.
/// </summary>
public interface IFamilyRelationRepository
{
    /// <summary>
    /// Retrieves a read-only list of all family relations asynchronously.
    /// </summary>
    /// <param name="search">Optional search term for filtering by name.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="active">Optional active status filter.</param>
    /// <param name="sort">Sorting parameters.</param>
    /// <param name="page">Page number for pagination.</param>
    /// <param name="limit">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of family relations and total count.</returns>
    Task<(IReadOnlyList<FamilyRelation> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific family relation by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the family relation.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The family relation if found; otherwise, null.</returns>
    Task<FamilyRelation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a family relation with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the family relation to check.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="excludeFamilyRelationId">Optional family relation ID to exclude from the check (useful during updates).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the name exists; otherwise, false.</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeFamilyRelationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new family relation to the repository context.
    /// </summary>
    /// <param name="familyRelation">The family relation entity to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddAsync(
        FamilyRelation familyRelation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing family relation as updated in the repository context.
    /// </summary>
    /// <param name="familyRelation">The family relation entity to update.</param>
    void Update(FamilyRelation familyRelation);

    /// <summary>
    /// Marks an existing family relation for removal from the repository context.
    /// </summary>
    /// <param name="familyRelation">The family relation entity to remove.</param>
    void Remove(FamilyRelation familyRelation);
}