namespace Sakolee.Api.Models.Dashboard;

// ---- Shared ----

public sealed record ActivityEntry(
    Guid Id, string Action, string? Actor, DateTime TimestampUtc, string? Notes);

// ---- Users ----

public sealed record UserKpisDto(
    int Total, int LoggedInToday, int ActiveThisWeek, int Inactive30Days, int PendingFirstLogin, int NewThisMonth);

public sealed record RoleCount(string Role, int Count);

public sealed record UserDashboardDto(
    UserKpisDto Kpis,
    IReadOnlyList<RoleCount> RoleDistribution,
    IReadOnlyList<ActivityEntry> ActivityFeed);

// ---- Platform (Super Admin) ----

public sealed record TenantKpisDto(
    int ActiveTenants, int InactiveTenants, int ArchivedTenants, int TotalUsers);

public sealed record TenantHealthRow(
    Guid TenantId, string TenantName, int ActiveUsers);

public sealed record GrowthPoint(string Date, int Tenants, int Users);

public sealed record OnboardingRow(Guid TenantId, string TenantName, bool MissingUsers);

public sealed record SystemAlert(string Id, string Type, string Severity, string Message, Guid? TenantId, string? TenantName);

public sealed record TenantCount(string TenantName, int Count);

public sealed record PlatformUserAnalyticsDto(
    int TotalActive, int LoggedInToday, int PendingFirstLogin, int NoRole, int NewThisMonth,
    IReadOnlyList<GrowthPoint> Growth, IReadOnlyList<TenantCount> ByTenant, IReadOnlyList<ActivityEntry> ActivityFeed);

public sealed record PlatformDashboardDto(
    TenantKpisDto TenantKpis,
    IReadOnlyList<TenantHealthRow> TenantHealth,
    IReadOnlyList<GrowthPoint> Growth,
    IReadOnlyList<OnboardingRow> Onboarding,
    IReadOnlyList<SystemAlert> SystemAlerts,
    PlatformUserAnalyticsDto UserAnalytics);

// ---- Studio (Enrollment & Studio Metrics) ----

/// <summary>
/// The studio dashboard's KPI band. A null metric has no data source yet (no online-registration,
/// portal-enrollment or request tables exist) — the card shows "—" rather than a made-up 0.
/// </summary>
public sealed record StudioMetricsDto(
    int TotalEnrollments,
    int RecentlyDropped,
    int ActiveFamilies,
    int ActiveStudents,
    int ActiveClasses,
    int ActiveStaff,
    int? NewOnlineRegistrations,
    int? PortalEnrollments,
    int? PendingRequests);

/// <summary>
/// One row of the studio dashboard's "Enrollment &amp; Student Activity" feed. <c>Id</c> is the
/// student's id; <c>Status</c> is "Confirmed" (active) or "Dropped" (deactivated).
/// </summary>
public sealed record EnrollmentActivityDto(
    Guid Id,
    string Student,
    string? Family,
    Guid? FamilyId,
    string? ClassName,
    Guid? ClassId,
    string Status,
    DateTime ChangedOnUtc);

// ---- Layout ----

public sealed record DashboardLayoutResponse(
    IReadOnlyList<string> WidgetOrder,
    IReadOnlyList<string> HiddenWidgets,
    IReadOnlyList<string> CollapsedWidgets);

public sealed class DashboardLayoutRequest
{
    public List<string> WidgetOrder { get; set; } = new();
    public List<string> HiddenWidgets { get; set; } = new();
    public List<string> CollapsedWidgets { get; set; } = new();
}
