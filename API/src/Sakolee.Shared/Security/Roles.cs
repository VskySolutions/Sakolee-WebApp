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

    /// <summary>
    /// Seeded the same way as <see cref="Student"/> — a family contact's own login account. Carries no
    /// platform permissions; a contact's access to their own family/student records is by ownership, not
    /// by RBAC grant.
    /// </summary>
    public const string Guardian = "Guardian";

    /// <summary>
    /// The role given to a family contact's login account when a family is created. Unlike the system
    /// roles above it is NOT a system role: BootstrapSeeder only creates it when missing, and its
    /// permission set is left for admins to tune. Its name is fixed (FamiliesController looks it up by
    /// name), so it cannot be renamed or deleted.
    /// </summary>
    public const string Parent = "Parent";
}
