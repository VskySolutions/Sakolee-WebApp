namespace Sakolee.Shared.Security;

/// <summary>
/// The platform system roles enforced by RBAC (SuperAdmin &gt; TenantAdmin). All other access is governed
/// by custom, permission-based roles.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";

    /// <summary>
    /// Seeded on every startup (BootstrapSeeder), same as the two roles above. Carries no platform
    /// permissions — a student's access to their own record is by ownership, not by RBAC grant — but is
    /// still a named system role so student accounts have something to hold and StudentsController can
    /// require it to exist.
    /// </summary>
    public const string Student = "Student";
}
