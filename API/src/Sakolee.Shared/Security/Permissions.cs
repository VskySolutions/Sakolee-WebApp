namespace Sakolee.Shared.Security;

/// <summary>
/// The platform permission catalogue. Custom roles (RBAC) are composed of these
/// permission keys; API endpoints are gated by them (replacing fixed role policies
/// over the RBAC rollout). Keys are stable strings of the form "area.action".
/// </summary>
public static class Permissions
{
    // Tenants
    public const string TenantsRead = "tenants.read";
    public const string TenantsWrite = "tenants.write";
    public const string TenantsArchive = "tenants.archive";

    // Persons (CRM master records — the precursor to a login account)
    public const string PersonsRead = "persons.read";
    public const string PersonsWrite = "persons.write";
    public const string PersonsDelete = "persons.delete";

    // Family Statuses
    public const string FamilyStatusesRead = "familyStatuses.read";
    public const string FamilyStatusesWrite = "familyStatuses.write";
    public const string FamilyStatusesDelete = "familyStatuses.delete";

    // Sessions (Dance academy session master records)
    public const string SessionsRead = "sessions.read";
    public const string SessionsWrite = "sessions.write";
    public const string SessionsDelete = "sessions.delete";

    // Students
    public const string StudentsRead = "students.read";
    public const string StudentsWrite = "students.write";
    public const string StudentsDelete = "students.delete";

    // Classes
    public const string ClassesRead = "classes.read";
    public const string ClassesWrite = "classes.write";
    public const string ClassesDelete = "classes.delete";

    // Class Categories
    public const string ClassCategoriesRead = "classCategories.read";
    public const string ClassCategoriesWrite = "classCategories.write"; 
    public const string ClassCategoriesDelete = "classCategories.delete";
    
    // Billing Cycles
    public const string BillingCyclesRead = "billingCycles.read"; 
    public const string BillingCyclesWrite = "billingCycles.write";
    public const string BillingCyclesDelete = "billingCycles.delete";

    // T-Shirt Sizes
    public const string TShirtSizesRead = "tShirtSizes.read";
    public const string TShirtSizesWrite = "tShirtSizes.write";
    public const string TShirtSizesDelete = "tShirtSizes.delete";

    // Users
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string UsersResetPassword = "users.reset_password";
    /// <summary>Create user groups and assign/remove users from them (and delete groups).</summary>
    public const string UsersGroupManagement = "users.groupManagement";

    // Roles (RBAC management)
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";
    public const string RolesAssign = "roles.assign";

    // Permission Groups (RBAC composition layer)
    /// <summary>Create, edit, and compose Permission Groups (and compose them into roles).</summary>
    public const string GroupsManage = "groups.manage";

    // SMTP Email Accounts
    /// <summary>Create, edit, delete, set-active, and test-send SMTP email accounts. Reads require only <see cref="UsersRead"/>.</summary>
    public const string EmailManage = "email.manage";

    // Universal Features (Phase 14)
    /// <summary>Manage tenant-wide Universal Feature settings: tags, shared saved views, tenant sticky
    /// notes, and Modified Log field configuration.</summary>
    public const string SettingsManage = "settings.manage";
    /// <summary>View, restore, and permanently delete soft-deleted records (Deleted Records Management).</summary>
    public const string RecordsAdminDelete = "records.adminDelete";

    // Option Sets (tenant-configurable input value lists)
    /// <summary>Read option lists and their values (for pickers and the management UI).</summary>
    public const string OptionSetsRead = "optionSets.read";
    /// <summary>Create, edit, reorder, and delete a tenant's own option lists and values.</summary>
    public const string OptionSetsManage = "optionSets.manage";

    // Locations
    public const string LocationsRead = "locations.read";
    public const string LocationsWrite = "locations.write";
    public const string LocationsDelete = "locations.delete";

    /// <summary>Every defined permission key.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        TenantsRead, TenantsWrite, TenantsArchive,
        PersonsRead, PersonsWrite, PersonsDelete,
        FamilyStatusesRead,FamilyStatusesWrite,FamilyStatusesDelete,
        SessionsRead, SessionsWrite, SessionsDelete,
        StudentsRead, StudentsWrite, StudentsDelete,
        ClassesRead, ClassesWrite, ClassesDelete,
        ClassCategoriesRead, ClassCategoriesWrite, ClassCategoriesDelete, 
        BillingCyclesRead, BillingCyclesWrite, BillingCyclesDelete,
        TShirtSizesRead,TShirtSizesWrite,TShirtSizesDelete,
        LocationsRead, LocationsWrite, LocationsDelete,
        UsersRead, UsersWrite, UsersResetPassword, UsersGroupManagement,
        RolesRead, RolesWrite, RolesAssign,
        GroupsManage,
        EmailManage,
        SettingsManage, RecordsAdminDelete,
        OptionSetsRead, OptionSetsManage
    };

    /// <summary>Permission sets for the seeded system roles.</summary>
    public static IReadOnlyList<string> ForSuperAdmin() => All;

    public static IReadOnlyList<string> ForTenantAdmin() => new[]
    {
        TenantsRead,
        // Deleting persons stays Super-Admin-only (PersonsDelete intentionally excluded here).
        PersonsRead, PersonsWrite,
        FamilyStatusesRead,FamilyStatusesWrite,FamilyStatusesDelete,
        SessionsRead, SessionsWrite, SessionsDelete,
        // Students are owned entirely within a tenant, so Tenant Admins get full CRUD.
        StudentsRead, StudentsWrite, StudentsDelete,
        // Classes carry no TenantId yet (see the Class entity remarks) but are administered the same way.
        ClassesRead, ClassesWrite, ClassesDelete,
        // Class Categories
        ClassCategoriesRead, ClassCategoriesWrite, ClassCategoriesDelete, 
        // Billing Cycles
        BillingCyclesRead, BillingCyclesWrite, BillingCyclesDelete,
        //T-Shirt Sizes
        TShirtSizesRead,TShirtSizesWrite,TShirtSizesDelete,
        // Location
        LocationsRead, LocationsWrite, LocationsDelete,
        // Users
        UsersRead, UsersWrite, UsersResetPassword, UsersGroupManagement,
        // Tenant Admins manage the roles of users in their OWN tenant, and build roles of their own to
        // assign. The permission alone is not the whole boundary. UsersController confines them to their
        // active tenant, refuses to grant the Super Admin role, refuses a Super Admin target, and refuses
        // a role another tenant owns; RolesController confines roles.write to the roles their own tenant
        // owns — the platform roles, this one included, stay a Super Admin's to change — and holds the
        // keys they may put in one inside the tenant ceiling (ADR-003).
        RolesRead, RolesWrite, RolesAssign,
        // Tenant Admins manage Permission Groups within their own tenant.
        GroupsManage,
        // Tenant Admins manage their tenant's SMTP email accounts.
        EmailManage,
        // Tenant Admins manage tenant-wide UF settings and the deleted-records lifecycle.
        SettingsManage, RecordsAdminDelete,
        // Tenant Admins manage their tenant's option lists.
        OptionSetsRead, OptionSetsManage
    };

    /// <summary>A student holds no platform permissions — access to their own record is by ownership.</summary>
    public static IReadOnlyList<string> ForStudent() => Array.Empty<string>();

    /// <summary>
    /// The seeded permission set for a system role name (SuperAdmin/TenantAdmin/Student), or an empty set
    /// for any other name (including custom roles). Used as the fallback when an assignment carries no
    /// explicit permission keys and when a caller holds only a role claim (API-key callers, pre-RBAC
    /// tokens).
    /// </summary>
    public static IReadOnlyList<string> ForSystemRole(string? roleName) => roleName switch
    {
        Roles.SuperAdmin => ForSuperAdmin(),
        Roles.TenantAdmin => ForTenantAdmin(),
        Roles.Student => ForStudent(),
        _ => Array.Empty<string>(),
    };
}
