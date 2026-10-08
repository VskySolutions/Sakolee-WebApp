using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>Data access for the <see cref="Policy"/> record (the <c>Policy</c> table) and its class mappings.</summary>
public interface IPolicyRepository
{
    #region List

    /// <summary>
    /// Paginated list (with class mappings) and optional free-text search over name and description.
    /// </summary>
    Task<(IReadOnlyList<Policy> Items, int Total)> ListAsync(
        string? search, Guid? tenantId, bool? showDeleted, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    /// <summary>The active tenant's active policies, in display order — the class form's picker options.</summary>
    Task<IReadOnlyList<Policy>> ListActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>The active tenant's policies attached to a class, in display order.</summary>
    Task<IReadOnlyList<Policy>> ListByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);

    #endregion

    #region Get

    /// <summary>Loads a policy (with its class mappings) by id, scoped to the active tenant.</summary>
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region ExistsByName

    /// <summary>Checks if a policy name already exists within the tenant.</summary>
    Task<bool> PolicyNameExistsAsync(
        Guid tenantId,
        string name,
        Guid? excludingPolicyId = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Create

    /// <summary>Adds a new policy; saved by the unit of work.</summary>
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);

    #endregion

    #region Update

    /// <summary>Marks a policy as updated; saved by the unit of work.</summary>
    void Update(Policy policy);

    #endregion

    #region Class Mappings

    /// <summary>Replaces the policy's class mappings with <paramref name="classIds"/>.</summary>
    void SetClasses(Policy policy, IEnumerable<Guid> classIds);

    /// <summary>
    /// Replaces the class's mappings to the active tenant's policies with <paramref name="policyIds"/>. Ids that
    /// are not a live policy of the tenant are ignored.
    /// </summary>
    Task SetPoliciesForClassAsync(Guid classId, IEnumerable<Guid> policyIds, CancellationToken cancellationToken = default);

    #endregion

    #region Delete

    /// <summary>Soft-deletes the policy and removes its class mappings.</summary>
    void Remove(Policy policy);

    #endregion
}
