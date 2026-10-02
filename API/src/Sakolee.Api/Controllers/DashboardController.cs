using System.Text.Json;
using Sakolee.Api.Dashboard;
using Sakolee.Api.Models.Dashboard;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Provides the read-only dashboard aggregations (WO-72): the user and studio dashboards for the
/// caller's tenant, the Super Admin platform overview, and each user's saved widget layout.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/dashboard")]
[Produces("application/json")]
[Tags("Dashboard")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status500InternalServerError)]
public sealed class DashboardController : ControllerBase
{
    #region Fields

    private readonly IDashboardQueryService _query;
    private readonly IDashboardCacheService _cache;
    private readonly IDashboardLayoutRepository _layouts;
    private readonly IUnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Dashboard controller.
    /// </summary>
    public DashboardController(
        IDashboardQueryService query,
        IDashboardCacheService cache,
        IDashboardLayoutRepository layouts,
        IUnitOfWork unitOfWork)
    {
        _query = query;
        _cache = cache;
        _layouts = layouts;
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region Users

    /// <summary>
    /// Gets the user dashboard (user KPIs, role distribution and activity feed) for the caller's
    /// tenant, or for the requested tenant when the caller is a Super Admin.
    /// </summary>
    [HttpGet("users")]
    [RequirePermission(Permissions.UsersRead)]
    [ProducesResponseType<ApiResponse<UserDashboardDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Users([FromQuery] string dateRange = "7d", [FromQuery] Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Build the user dashboard for the resolved tenant scope.
        var data = await _query.GetUsersAsync(ResolveScope(tenantId), dateRange, cancellationToken);
        // Return the user dashboard data.
        return Ok(ApiResponseFactory.Success(data, "User dashboard retrieved."));
    }

    #endregion

    #region Studio Metrics

    /// <summary>
    /// Gets the "Enrollment &amp; Studio Metrics" KPI band (enrollments, active families, students,
    /// classes and staff) for the caller's tenant, or for the requested tenant when the caller is a
    /// Super Admin.
    /// </summary>
    [HttpGet("studio-metrics")]
    [ProducesResponseType<ApiResponse<StudioMetricsDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> StudioMetrics([FromQuery] Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Count the studio metrics for the resolved tenant scope.
        var data = await _query.GetStudioMetricsAsync(ResolveScope(tenantId), cancellationToken);
        // Return the studio metrics.
        return Ok(ApiResponseFactory.Success(data, "Studio metrics retrieved."));
    }

    #endregion

    #region Enrollment Activity

    /// <summary>
    /// Gets the "Enrollment &amp; Student Activity" feed: the most recent enrollment status changes
    /// (newest first) for the caller's tenant, or for the requested tenant when the caller is a
    /// Super Admin.
    /// </summary>
    [HttpGet("enrollment-activity")]
    [ProducesResponseType<ApiResponse<IReadOnlyList<EnrollmentActivityDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnrollmentActivity([FromQuery] int limit = 5, [FromQuery] Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Keep the feed to a sensible size.
        limit = Math.Clamp(limit, 1, 50);
        // Load the latest enrollment changes for the resolved tenant scope.
        var data = await _query.GetEnrollmentActivityAsync(ResolveScope(tenantId), limit, cancellationToken);
        // Return the activity feed.
        return Ok(ApiResponseFactory.Success(data, "Enrollment activity retrieved."));
    }

    #endregion

    #region Platform

    /// <summary>
    /// Gets the platform overview (tenant KPIs, tenant health, growth, onboarding and user analytics).
    /// Restricted to Super Admins; results are cached unless the
    /// <c>X-Dashboard-Force-Refresh</c> header asks for a fresh build.
    /// </summary>
    [HttpGet("platform")]
    [ProducesResponseType<ApiResponse<PlatformDashboardDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Platform([FromQuery] string dateRange = "7d", CancellationToken cancellationToken = default)
    {
        // Only Super Admins may see the platform-wide overview.
        if (!User.IsSuperAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponseFactory.Forbidden("Platform dashboard is restricted to Super Admins."));
        }

        // Bypass the cache when the client explicitly requests a refresh.
        var forceRefresh = TruthyHeader(Request.Headers["X-Dashboard-Force-Refresh"]);
        // Serve the cached overview for this date range, building it on a miss or a forced refresh.
        var data = await _cache.GetOrAddAsync(
            $"dashboard:platform:{dateRange}",
            forceRefresh,
            () => _query.GetPlatformAsync(dateRange, forceRefresh, cancellationToken));

        // Return the platform dashboard data.
        return Ok(ApiResponseFactory.Success(data, "Platform dashboard retrieved."));
    }

    #endregion

    #region Layout

    /// <summary>
    /// Gets the caller's saved dashboard widget layout, or the default layout for their dashboard
    /// role when nothing has been saved yet.
    /// </summary>
    [HttpGet("layout")]
    [ProducesResponseType<ApiResponse<DashboardLayoutResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLayout(CancellationToken cancellationToken)
    {
        // Get the current user from the token.
        var userId = User.GetUserId();
        if (userId is not { } uid)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponseFactory.Unauthorized("No user in token."));
        }

        // Return the user's own layout when one has been saved.
        var saved = await _layouts.GetByUserAsync(uid, cancellationToken);
        if (saved is not null)
        {
            return Ok(ApiResponseFactory.Success(
                new DashboardLayoutResponse(saved.WidgetOrder, saved.HiddenWidgets, saved.CollapsedWidgets),
                "Layout retrieved."));
        }

        // Otherwise fall back to the default layout for the user's dashboard role.
        var role = ResolveDashboardRole();
        return Ok(ApiResponseFactory.Success(
            new DashboardLayoutResponse(
                DashboardDefaultLayouts.For(role),
                DashboardDefaultLayouts.DefaultHiddenFor(role),
                Array.Empty<string>()),
            "Default layout retrieved."));
    }

    /// <summary>
    /// Saves the caller's dashboard widget layout (order, hidden and collapsed widgets),
    /// replacing any layout saved before.
    /// </summary>
    [HttpPut("layout")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveLayout([FromBody] DashboardLayoutRequest body, CancellationToken cancellationToken)
    {
        // Get the current user from the token.
        var userId = User.GetUserId();
        if (userId is not { } uid)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponseFactory.Unauthorized("No user in token."));
        }

        // Build the layout record, storing each widget list as JSON.
        var layout = new DashboardLayout
        {
            Id = Guid.NewGuid(),
            UserId = uid,
            WidgetOrderJson = JsonSerializer.Serialize(body.WidgetOrder ?? new List<string>()),
            HiddenWidgetsJson = JsonSerializer.Serialize(body.HiddenWidgets ?? new List<string>()),
            CollapsedWidgetsJson = JsonSerializer.Serialize(body.CollapsedWidgets ?? new List<string>()),
        };
        // Insert or replace the user's layout and persist it.
        await _layouts.UpsertAsync(layout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Confirm the layout was saved.
        return Ok(ApiResponseFactory.Success(new { saved = true }, "Layout saved."));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Resolves the tenant a dashboard query is scoped to. Super Admins may target any tenant
    /// explicitly; with none requested they get the tenant they are currently viewing (the claim
    /// follows the Super-Admin tenant scope). Everyone else always gets their active tenant.
    /// </summary>
    private Guid? ResolveScope(Guid? requestedTenantId)
        => (User.IsSuperAdmin() ? requestedTenantId : null) ?? User.GetActiveTenantId();

    /// <summary>
    /// Resolves the layout tier: Super Admin, else Tenant Admin (users.read + tenants.read), else
    /// Common.
    /// </summary>
    private DashboardRole ResolveDashboardRole()
    {
        // Super Admins get the platform layout.
        if (User.IsSuperAdmin())
        {
            return DashboardRole.SuperAdmin;
        }
        // Users who can read both users and tenants get the Tenant Admin layout.
        if (User.HasPermission(Permissions.UsersRead) && User.HasPermission(Permissions.TenantsRead))
        {
            return DashboardRole.TenantAdmin;
        }
        // Everyone else gets the common layout.
        return DashboardRole.Common;
    }

    /// <summary>
    /// Treats a header as "true" unless it is missing, blank, "false" or "0".
    /// </summary>
    private static bool TruthyHeader(string? value)
        => !string.IsNullOrWhiteSpace(value)
            && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(value, "0", StringComparison.Ordinal);

    #endregion
}
