namespace Sakolee.Domain.Entities;

/// <summary>
/// A platform user account. Holds authentication/authorization concerns only; personal
/// profile information lives on the associated <see cref="Person"/> (WO-61). Credentials
/// are PBKDF2-hashed; <see cref="TokenVersion"/> is incremented on password change,
/// deactivation, email change, and logout to invalidate outstanding JWTs.
/// </summary>
public class User : AuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>Unique login email (also the user's email address).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Display identity used by auth/UI; the full profile lives on <see cref="Person"/>.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>The associated person master record. Every user has one (WO-61).</summary>
    public Guid? PersonId { get; set; }

    public Person? Person { get; set; }

    /// <summary>Base64 PBKDF2 hash of the password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Base64 per-user salt.</summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>
    /// The last temporary password minted for this user (invite, tenant-admin creation, or an admin
    /// reset), encrypted at rest — never stored in plaintext. Lets "resend credentials" actions re-email
    /// the same password without minting a new one. Cleared as soon as the user signs in and sets their
    /// own password (<see cref="MustChangePassword"/> goes false), after which nothing is left to resend.
    /// </summary>
    public string? EncryptedTemporaryPassword { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True for a tenant's default administrator account (minted alongside the tenant — see
    /// <see cref="Tenant.PersonId"/>). Guarantees the tenant always keeps at least one working admin
    /// login: <c>UsersController</c> refuses to deactivate this user or remove its tenant role assignment.
    /// There is no user-delete endpoint on the platform, so this is the closest equivalent to "cannot be
    /// deleted".
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>Forces a password change before non-auth API access (new accounts).</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Session-invalidation counter embedded in issued JWTs.</summary>
    public int TokenVersion { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>The user's tenant/role assignments.</summary>
    public ICollection<UserTenantRole> TenantRoles { get; set; } = new List<UserTenantRole>();

    /// <summary>The user's group memberships (tenant-scoped segmentation).</summary>
    public ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();
}
