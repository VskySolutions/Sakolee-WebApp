using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Provides data access operations for Hear About Us records.
/// </summary>
public interface IHearAboutUsRepository
{
    /// <summary>
    /// Gets all non-deleted Hear About Us records belonging to the given tenant.
    /// Supports searching and sorting by name and audit dates.
    /// </summary>
    Task<(IReadOnlyList<HearAboutUs> Items, int Total)> ListByTenantAsync(Guid tenantId, string? name = null,bool showDeleted = false, bool? active = null, string? search = null, string? sortBy = null, bool descending = false, int page = 1,int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a non-deleted Hear About Us record by identifier
    /// for the specified tenant.
    /// </summary>
    Task<HearAboutUs?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an active Hear About Us record with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a Hear About Us record to the current DbContext.
    /// </summary>
    Task AddAsync(HearAboutUs hearAboutUs,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a Hear About Us record as modified.
    /// </summary>
    void Update(HearAboutUs hearAboutUs);

    /// <summary>
    /// Removes a Hear About Us record from the current DbContext.
    /// </summary>
    void Remove(HearAboutUs hearAboutUs);

}