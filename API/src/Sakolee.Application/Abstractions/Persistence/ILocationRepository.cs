using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for managing Location data operations.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Retrieves a read-only list of all locations asynchronously.
    /// </summary>
    /// <param name="search">Optional search term.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="active">Optional active status filter.</param>
    /// <param name="showDeleted">Optional flag to show deleted records.</param>
    /// <param name="sort">Sorting parameters.</param>
    /// <param name="page">Page number.</param>
    /// <param name="limit">Page size limit.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of locations and total count.</returns>
    Task<(IReadOnlyList<Location> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         bool? showDeleted,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific location by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the location.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The location if found; otherwise, null.</returns>
    Task<Location?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a location with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the location to check.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="excludeLocationId">Optional location ID to exclude from the check (useful during updates).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the name exists; otherwise, false.</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeLocationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new location to the repository context.
    /// </summary>
    /// <param name="location">The location entity to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddAsync(
        Location location,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing location as updated in the repository context.
    /// </summary>
    /// <param name="location">The location entity to update.</param>
    void Update(Location location);

    /// <summary>
    /// Marks an existing location for removal from the repository context.
    /// </summary>
    /// <param name="location">The location entity to remove.</param>
    void Remove(Location location);
}