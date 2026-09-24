using System.Security.Claims;
using Sakolee.Shared.Security;

namespace Sakolee.Api.Security;

/// <summary>Convenience accessors for the platform JWT claims on the current principal.</summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirst(ClaimTypeNames.Subject)?.Value, out var id) ? id : null;

    public static Guid? GetActiveTenantId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirst(ClaimTypeNames.ActiveTenantId)?.Value, out var id) ? id : null;

    /// <summary>The caller's first <c>role</c> claim (a user may hold several — see <see cref="GetRoles"/>).</summary>
    public static string? GetRole(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypeNames.Role)?.Value;

    /// <summary>Every <c>role</c> claim the caller carries (multi-role assignments emit one per role name).</summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypeNames.Role).Select(c => c.Value);

    public static bool IsSuperAdmin(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypeNames.Role).Any(c => string.Equals(c.Value, Roles.SuperAdmin, StringComparison.Ordinal));

    /// <summary>
    /// True when every role the caller holds in the active tenant is a family-contact role
    /// (<see cref="Roles.Parent"/>/<see cref="Roles.Guardian"/>). Such a caller only ever sees their own
    /// family's records, whatever permissions those roles carry; a caller who also holds any other role
    /// (Staff, Administrator, …) is not limited this way.
    /// </summary>
    public static bool IsFamilyContactOnly(this ClaimsPrincipal principal)
    {
        var roles = principal.GetRoles().ToList();
        return roles.Count > 0 && roles.All(role =>
            string.Equals(role, Roles.Parent, StringComparison.Ordinal)
            || string.Equals(role, Roles.Guardian, StringComparison.Ordinal));
    }

    /// <summary>
    /// True when the caller holds the given permission — either via an explicit permission claim or,
    /// as a fallback (API-key/pre-RBAC callers).
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        => principal.HasClaim(ClaimTypeNames.Permission, permission)
            || principal.GetRoles().Any(role => Permissions.ForSystemRole(role).Contains(permission));
}
