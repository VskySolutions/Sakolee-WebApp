using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for managing BankMethod data operations.
/// </summary>
public interface IBillingMethodRepository
{
    /// <summary>
    /// Retrieves a read-only list of all bank methods asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of bank methods.</returns>
    Task<(IReadOnlyList<BillingMethod> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific bank method by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the bank method.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The bank method if found; otherwise, null.</returns>
    Task<BillingMethod?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a bank method with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the bank method to check.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="excludeBankMethodId">Optional bank method ID to exclude from the check (useful during updates).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the name exists; otherwise, false.</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeBankMethodId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bank method to the repository context.
    /// </summary>
    /// <param name="bankMethod">The bank method entity to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddAsync(
        BillingMethod bankMethod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing bank method as updated in the repository context.
    /// </summary>
    /// <param name="bankMethod">The bank method entity to update.</param>
    void Update(BillingMethod bankMethod);

    /// <summary>
    /// Marks an existing bank method for removal from the repository context.
    /// </summary>
    /// <param name="bankMethod">The bank method entity to remove.</param>
    void Remove(BillingMethod bankMethod);
}