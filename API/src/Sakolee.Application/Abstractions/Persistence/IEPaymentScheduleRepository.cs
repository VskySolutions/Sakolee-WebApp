using Sakolee.Domain.Entities;
namespace Sakolee.Application.Abstractions.Persistence;
/// <summary>
/// Provides data access operations for E-Payment Schedule records.
/// </summary>
public interface IEPaymentScheduleRepository
{
    /// <summary>
    /// Gets all non-deleted E-Payment Schedule records belonging to the given tenant.
    /// Supports searching and sorting by name and audit dates.
    /// </summary>
    Task<(IReadOnlyList<EPaymentSchedule> Items, int Total)> ListByTenantAsync(Guid tenantId,string? name = null,bool showDeleted = false,bool? active = null,string? search = null,string? sortBy = null,bool descending = false,int page = 1,int limit = 20,CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a non-deleted E-Payment Schedule record by identifier
    /// for the specified tenant.
    /// </summary>
    Task<EPaymentSchedule?> GetByIdAsync(Guid id,Guid tenantId,CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an active E-Payment Schedule record with the specified
    /// name already exists for the tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(Guid tenantId,string name,Guid? excludeId = null,CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an E-Payment Schedule record to the current DbContext.
    /// </summary>
    Task AddAsync(EPaymentSchedule ePaymentSchedule,CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an E-Payment Schedule record as modified.
    /// </summary>
    void Update(EPaymentSchedule ePaymentSchedule);

    /// <summary>
    /// Removes an E-Payment Schedule record from the current DbContext.
    /// </summary>
    void Remove(EPaymentSchedule ePaymentSchedule);
}
