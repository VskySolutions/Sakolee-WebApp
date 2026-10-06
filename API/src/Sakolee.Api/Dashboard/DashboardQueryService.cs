using Sakolee.Api.Models.Dashboard;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Sakolee.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Api.Dashboard;

/// <summary>Builds the dashboard read models.</summary>
public sealed class DashboardQueryService : IDashboardQueryService
{
    private readonly SakoleeDbContext _db;
    private readonly ITenantRepository _tenants;

    public DashboardQueryService(
        SakoleeDbContext db,
        ITenantRepository tenants)
    {
        _db = db;
        _tenants = tenants;
    }

    // ---- Scoping helpers ----

    private IQueryable<User> UsersScoped(Guid? tenantId)
    {
        var q = _db.Users.IgnoreQueryFilters().Where(u => !u.Deleted);
        return tenantId is { } tid ? q.Where(u => u.TenantRoles.Any(r => r.TenantId == tid && !r.Deleted)) : q;
    }

    // ---- Users ----

    public async Task<UserDashboardDto> GetUsersAsync(Guid? tenantId, string dateRange, CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var users = await UsersScoped(tenantId)
            .Select(u => new { u.Id, u.MustChangePassword, u.CreatedOnUtc, u.IsActive })
            .ToListAsync(cancellationToken);

        // LoggedInToday / ActiveThisWeek / Inactive30Days are not trackable (no last-login timestamp): return 0.
        var kpis = new UserKpisDto(
            Total: users.Count,
            LoggedInToday: 0,
            ActiveThisWeek: 0,
            Inactive30Days: 0,
            PendingFirstLogin: users.Count(u => u.MustChangePassword),
            NewThisMonth: users.Count(u => u.CreatedOnUtc >= monthStart));

        var roleDistribution = await RoleDistributionAsync(tenantId, cancellationToken);

        // No user-activity audit source; an empty feed is acceptable for this WO.
        return new UserDashboardDto(kpis, roleDistribution, Array.Empty<ActivityEntry>());
    }

    private async Task<IReadOnlyList<RoleCount>> RoleDistributionAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var assignments = await _db.UserTenantRoles.IgnoreQueryFilters()
            .Where(r => !r.Deleted)
            .Where(r => tenantId == null || r.TenantId == tenantId)
            .Where(r => !r.User!.Deleted)
            .Select(r => new { r.UserId, RoleName = r.RoleEntity != null ? r.RoleEntity.Name : null, r.Role })
            .ToListAsync(cancellationToken);

        // One role label per user (first assignment wins); users with no assignment are "Unassigned".
        var allUsers = await UsersScoped(tenantId).Select(u => u.Id).ToListAsync(cancellationToken);
        var byUser = assignments
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.First().RoleName ?? g.First().Role.ToString());

        var counts = new Dictionary<string, int>();
        foreach (var userId in allUsers)
        {
            var label = byUser.TryGetValue(userId, out var name) && !string.IsNullOrWhiteSpace(name) ? name : "Unassigned";
            counts[label] = counts.GetValueOrDefault(label) + 1;
        }
        return counts.Select(kv => new RoleCount(kv.Key, kv.Value)).OrderByDescending(r => r.Count).ToList();
    }

    // ---- Studio (Enrollment & Studio Metrics) ----

    /// <summary>The role whose active holders count as staff (the same role the Staff list and instructor picker use).</summary>
    private const string StaffRole = "Staff";

    /// <summary>How far back a deactivated student still counts as "recently dropped".</summary>
    private const int RecentlyDroppedDays = 30;

    public async Task<StudioMetricsDto> GetStudioMetricsAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var students = await StudentsScopedAsync(tenantId, cancellationToken);

        // Enrollments are (active student, class) pairs — StudentClasses, falling back to Student.ClassId for
        // a student with no rows there; dropped = deactivated within the window, the closest signal there
        // is without an enrollment history table.
        var droppedSince = DateTime.UtcNow.AddDays(-RecentlyDroppedDays);
        var activeIds = students.Where(s => s.Active).Select(s => s.Id).ToList();
        var classCounts = (await _db.StudentClasses.IgnoreQueryFilters()
                .Where(sc => !sc.Deleted && activeIds.Contains(sc.StudentId))
                .Select(sc => new { sc.StudentId, sc.ClassId })
                .Distinct()
                .ToListAsync(cancellationToken))
            .GroupBy(sc => sc.StudentId)
            .ToDictionary(g => g.Key, g => g.Count());
        var totalEnrollments = students
            .Where(s => s.Active)
            .Sum(s => classCounts.TryGetValue(s.Id, out var count) ? count : s.ClassId.HasValue ? 1 : 0);
        var recentlyDropped = students.Count(s => !s.Active && s.UpdatedOnUtc >= droppedSince);
        var activeStudents = students.Count(s => s.Active);

        var activeFamilies = await _db.Families.IgnoreQueryFilters()
            .Where(f => !f.Deleted && f.Active)
            .Where(f => tenantId == null || f.TenantId == tenantId)
            .CountAsync(cancellationToken);

        // Classes carry no TenantId yet and are listed platform-wide (see Class remarks), so this
        // matches what the Classes list shows.
        var activeClasses = await _db.Classes.IgnoreQueryFilters()
            .CountAsync(c => !c.Deleted && c.Active, cancellationToken);

        var activeStaff = await UsersScoped(tenantId)
            .Where(u => u.IsActive)
            .CountAsync(u => u.TenantRoles.Any(r =>
                !r.Deleted
                && (tenantId == null || r.TenantId == tenantId)
                && r.RoleEntity != null && r.RoleEntity.Name == StaffRole), cancellationToken);

        return new StudioMetricsDto(
            totalEnrollments,
            recentlyDropped,
            activeFamilies,
            activeStudents,
            activeClasses,
            activeStaff,
            NewOnlineRegistrations: null,
            PortalEnrollments: null,
            PendingRequests: null);
    }

    /// <summary>
    /// The most recent enrollment status changes: students assigned to a class, newest change first.
    /// An active student reads as "Confirmed" and a deactivated one as "Dropped" — there is no
    /// enrollment history table, so the student row's own last change stands in for the event.
    /// </summary>
    public async Task<IReadOnlyList<EnrollmentActivityDto>> GetEnrollmentActivityAsync(Guid? tenantId, int limit, CancellationToken cancellationToken)
    {
        var recent = (await StudentsScopedAsync(tenantId, cancellationToken))
            .Where(s => s.ClassId.HasValue)
            .OrderByDescending(s => s.UpdatedOnUtc ?? s.CreatedOnUtc)
            .Take(limit)
            .ToList();
        if (recent.Count == 0)
        {
            return Array.Empty<EnrollmentActivityDto>();
        }

        // Names live on the linked Person, Family and Class rows; look up only the ones on show.
        var personIds = recent.Where(s => s.PersonId.HasValue).Select(s => s.PersonId!.Value).Distinct().ToList();
        var familyIds = recent.Where(s => s.FamilyId.HasValue).Select(s => s.FamilyId!.Value).Distinct().ToList();
        var classIds = recent.Select(s => s.ClassId!.Value).Distinct().ToList();

        var people = await _db.Persons.IgnoreQueryFilters()
            .Where(p => personIds.Contains(p.Id))
            .Select(p => new { p.Id, p.DisplayName, p.FirstName, p.LastName })
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var families = await _db.Families.IgnoreQueryFilters()
            .Where(f => familyIds.Contains(f.Id))
            .Select(f => new { f.Id, f.FamilyName, f.FirstName })
            .ToDictionaryAsync(f => f.Id, cancellationToken);
        var classes = await _db.Classes.IgnoreQueryFilters()
            .Where(c => classIds.Contains(c.Id))
            .Select(c => new { c.Id, c.ClassName })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return recent.Select(s =>
        {
            var person = s.PersonId is { } pid ? people.GetValueOrDefault(pid) : null;
            var studentName = person is null
                ? "Unknown student"
                : !string.IsNullOrWhiteSpace(person.DisplayName)
                    ? person.DisplayName
                    : $"{person.FirstName} {person.LastName}".Trim();

            // "Miller (David)": the family name with its primary contact's first name, when known.
            var family = s.FamilyId is { } fid ? families.GetValueOrDefault(fid) : null;
            var familyName = family is null
                ? null
                : string.IsNullOrWhiteSpace(family.FirstName)
                    ? family.FamilyName
                    : $"{family.FamilyName} ({family.FirstName})";

            return new EnrollmentActivityDto(
                s.Id,
                studentName,
                familyName,
                s.FamilyId,
                classes.GetValueOrDefault(s.ClassId!.Value)?.ClassName,
                s.ClassId,
                s.Active ? "Confirmed" : "Dropped",
                s.UpdatedOnUtc ?? s.CreatedOnUtc);
        }).ToList();
    }

    /// <summary>
    /// The tenant's non-deleted students (all tenants when <paramref name="tenantId"/> is null).
    /// Students carry no TenantId — they are scoped through their Person's TenantPersonMapping, and
    /// PersonId is nvarchar on that legacy table, so (as StudentRepository does) the tenant's person
    /// ids are resolved first and students filtered in memory.
    /// </summary>
    private async Task<List<Student>> StudentsScopedAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var students = await _db.Students.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => !s.Deleted)
            .ToListAsync(cancellationToken);
        if (tenantId is not { } tid)
        {
            return students;
        }

        var personIds = new HashSet<Guid>(await _db.TenantPersonMappings.IgnoreQueryFilters()
            .Where(m => !m.Deleted && m.TenantId == tid)
            .Select(m => m.PersonId)
            .ToListAsync(cancellationToken));
        return students.Where(s => s.PersonId.HasValue && personIds.Contains(s.PersonId.Value)).ToList();
    }

    // ---- Platform (Super Admin) ----

    public async Task<PlatformDashboardDto> GetPlatformAsync(string dateRange, bool forceRefresh, CancellationToken cancellationToken)
        => await BuildPlatformAsync(dateRange, cancellationToken);

    private async Task<PlatformDashboardDto> BuildPlatformAsync(string dateRange, CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var tenants = await _tenants.ListAsync(cancellationToken);
        var tenantNames = tenants.ToDictionary(t => t.Id, t => t.Name);

        var activeTenants = tenants.Count(t => t.Status == TenantStatus.Active);
        var inactiveTenants = tenants.Count(t => t.Status == TenantStatus.Inactive);
        var archivedTenants = tenants.Count(t => t.Status == TenantStatus.Archived);

        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => !u.Deleted, cancellationToken);

        var tenantKpis = new TenantKpisDto(activeTenants, inactiveTenants, archivedTenants, totalUsers);

        var tenantHealth = await TenantHealthAsync(tenants, cancellationToken);
        var growth = await GrowthAsync(tenants, cancellationToken);
        var onboarding = await OnboardingAsync(tenants, cancellationToken);
        var userAnalytics = await PlatformUserAnalyticsAsync(tenantNames, monthStart, cancellationToken);

        return new PlatformDashboardDto(
            tenantKpis, tenantHealth, growth, onboarding, Array.Empty<SystemAlert>(), userAnalytics);
    }

    private async Task<IReadOnlyList<TenantHealthRow>> TenantHealthAsync(IReadOnlyList<Tenant> tenants, CancellationToken cancellationToken)
    {
        var rows = new List<TenantHealthRow>();
        foreach (var t in tenants)
        {
            var activeUsers = await UsersScoped(t.Id).CountAsync(u => u.IsActive, cancellationToken);
            rows.Add(new TenantHealthRow(t.Id, t.Name, activeUsers));
        }
        return rows;
    }

    private async Task<IReadOnlyList<GrowthPoint>> GrowthAsync(IReadOnlyList<Tenant> tenants, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.Date;
        var start = now.AddDays(-90);
        var userDates = await _db.Users.IgnoreQueryFilters().Where(u => !u.Deleted).Select(u => u.CreatedOnUtc).ToListAsync(cancellationToken);
        var tenantDates = tenants.Select(t => t.CreatedDate == default ? t.CreatedOnUtc : t.CreatedDate).ToList();

        var points = new List<GrowthPoint>();
        for (var d = start; d <= now; d = d.AddDays(1))
        {
            var dayEnd = d.AddDays(1);
            points.Add(new GrowthPoint(
                d.ToString("yyyy-MM-dd"),
                tenantDates.Count(td => td < dayEnd),
                userDates.Count(ud => ud < dayEnd)));
        }
        return points;
    }

    private async Task<IReadOnlyList<OnboardingRow>> OnboardingAsync(IReadOnlyList<Tenant> tenants, CancellationToken cancellationToken)
    {
        var rows = new List<OnboardingRow>();
        foreach (var t in tenants)
        {
            var missingUsers = !await UsersScoped(t.Id).AnyAsync(cancellationToken);
            rows.Add(new OnboardingRow(t.Id, t.Name, missingUsers));
        }
        return rows;
    }

    private async Task<PlatformUserAnalyticsDto> PlatformUserAnalyticsAsync(
        IReadOnlyDictionary<Guid, string> tenantNames, DateTime monthStart, CancellationToken cancellationToken)
    {
        var totalActive = await _db.Users.IgnoreQueryFilters().CountAsync(u => !u.Deleted && u.IsActive, cancellationToken);
        var pendingFirstLogin = await _db.Users.IgnoreQueryFilters().CountAsync(u => !u.Deleted && u.MustChangePassword, cancellationToken);
        var newThisMonth = await _db.Users.IgnoreQueryFilters().CountAsync(u => !u.Deleted && u.CreatedOnUtc >= monthStart, cancellationToken);

        // Users with no (non-deleted) tenant-role assignment.
        var noRole = await _db.Users.IgnoreQueryFilters()
            .CountAsync(u => !u.Deleted && !u.TenantRoles.Any(r => !r.Deleted), cancellationToken);

        // ByTenant: count of users per tenant via assignments.
        var byTenantRaw = await _db.UserTenantRoles.IgnoreQueryFilters()
            .Where(r => !r.Deleted && !r.User!.Deleted)
            .GroupBy(r => r.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Select(x => x.UserId).Distinct().Count() })
            .ToListAsync(cancellationToken);
        var byTenant = byTenantRaw
            .Select(x => new TenantCount(tenantNames.GetValueOrDefault(x.TenantId, x.TenantId.ToString()), x.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        var growth = await GrowthAsync(await _tenants.ListAsync(cancellationToken), cancellationToken);

        // LoggedInToday not trackable (no last-login timestamp): 0. ActivityFeed empty (no user audit source).
        return new PlatformUserAnalyticsDto(
            totalActive, LoggedInToday: 0, pendingFirstLogin, noRole, newThisMonth, growth, byTenant, Array.Empty<ActivityEntry>());
    }
}
