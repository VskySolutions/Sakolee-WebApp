using global::Hangfire;
using global::Hangfire.Dashboard;
using Sakolee.Infrastructure.Hangfire;
using Sakolee.Shared.Configuration;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Hangfire;

/// <summary>
/// Registers Hangfire storage so the Integration API can host the monitoring dashboard at
/// <c>/hangfire</c>, restricted to admin roles via the authorization pipeline.
/// </summary>
public static class HangfireDashboardExtensions
{
    public static IServiceCollection AddSakoleeHangfireDashboard(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (HangfireStorageConfigurator.IsConfigured(configuration))
        {
            services.AddHangfire(config => HangfireStorageConfigurator.Configure(config, configuration));
        }

        return services;
    }

    public static IApplicationBuilder UseSakoleeHangfireDashboard(
        this WebApplication app,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(ConfigurationSections.Hangfire).Get<HangfireOptions>()
            ?? new HangfireOptions();

        if (!options.DashboardEnabled || !HangfireStorageConfigurator.IsConfigured(configuration))
        {
            return app;
        }

        // ASP.NET authorization (TenantAdminOrAbove) gates the dashboard; Hangfire's
        // own local-only filter is cleared so the policy is the single gate.
        app.MapHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = Array.Empty<IDashboardAuthorizationFilter>(),
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrAbove);

        return app;
    }
}
