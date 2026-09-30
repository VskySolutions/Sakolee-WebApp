using Sakolee.Domain.Enums;
using Sakolee.Application.Common;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// One soft-deleted record surfaced in the Deleted Records Management list. Settable properties rather
/// than a positional constructor: the list is sorted after projecting, and EF can only translate an
/// ORDER BY on a member of a projection built by member initialisation.
/// </summary>
public sealed record DeletedRecordRow
{
    /// <summary>The deleted record's id.</summary>
    public Guid EntityId { get; init; }

    /// <summary>A human-readable identifier (e.g. request number, group name) used for display and the hard-delete confirmation token.</summary>
    public string Identity { get; init; } = string.Empty;

    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The user who deleted it (resolved to a name by the controller).</summary>
    public Guid? DeletedById { get; init; }

    /// <summary>When it was soft-deleted.</summary>
    public DateTime? DeletedOnUtc { get; init; }
}

/// <summary>
/// Generic access to soft-deleted records across the entity types that support Deleted Records
/// Management.
/// </summary>
public interface IDeletedRecordsRepository
{
    /// <summary>Whether Deleted Records Management is implemented for the given entity type.</summary>
    bool IsSupported(EntityType entityType);

    /// <summary>Paginated soft-deleted records of an entity type, scoped to <paramref name="tenantId"/> when given (else the ambient tenant).</summary>
    Task<(IReadOnlyList<DeletedRecordRow> Items, int Total)> ListDeletedAsync(
        EntityType entityType, Guid? tenantId, SortRequest sort, int page, int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The identity string of a soft-deleted record (for confirmation-token validation), or null when
    /// not found.
    /// </summary>
    Task<string?> GetDeletedIdentityAsync(EntityType entityType, Guid entityId, Guid? tenantId, CancellationToken cancellationToken = default);

    /// <summary>Restores a soft-deleted record and its soft-deleted UF rows.</summary>
    Task<bool> RestoreAsync(EntityType entityType, Guid entityId, Guid? tenantId, CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a soft-deleted record and cascades all its UF rows.</summary>
    Task<bool> HardDeleteAsync(EntityType entityType, Guid entityId, Guid? tenantId, CancellationToken cancellationToken = default);

    /// <summary>Counts records past their retention period, keyed by supported entity type.</summary>
    Task<IReadOnlyDictionary<EntityType, int>> CountOverdueAsync(int retentionDays, Guid? tenantId, CancellationToken cancellationToken = default);
}
