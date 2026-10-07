namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Seeds a newly created tenant's master tables (Account Types, Billing Cycles, Locations, Class Rooms, …)
/// by copying the template tenant's rows marked <c>IsSystem</c>. The template tenant is the one named by
/// <c>TenantProvisioning:TemplateTenantIdentifier</c> (falling back to the bootstrap tenant, "system").
/// </summary>
public interface ITenantMasterDataProvisioner
{
    /// <summary>
    /// Stages a copy of every live <c>IsSystem</c> template row for <paramref name="tenantId"/>. Returns the
    /// number of rows staged (0 when no template tenant is configured or found). Does not save.
    /// </summary>
    Task<int> CopyDefaultsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
