using Sakolee.Api.Models.Dashboard;

namespace Sakolee.Api.Dashboard;

/// <summary>Aggregates the dashboard read models from across the platform's data sources (users, tenants).</summary>
public interface IDashboardQueryService
{
    Task<UserDashboardDto> GetUsersAsync(Guid? tenantId, string dateRange, CancellationToken cancellationToken);

    Task<PlatformDashboardDto> GetPlatformAsync(string dateRange, bool forceRefresh, CancellationToken cancellationToken);
}
