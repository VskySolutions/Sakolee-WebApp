using Sakolee.Api.Models.Profile;

namespace Sakolee.Api.Models.Tenants;

public sealed class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = "UTC";

    // ---- Tenant's own address (optional; stored in the Addresses table, referenced by Tenant.AddressId) ----
    public AddressInput? Address { get; set; }

    // ---- Default Administrator — minted alongside the tenant as a Person + User (WO-61) ----
    /// <summary>The administrator's given name (Persons table).</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>The administrator's family name (Persons table).</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Login email for the administrator account; also the person's primary email.</summary>
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? CountryCode { get; set; }
}

public sealed class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TimeZoneId { get; set; }

    // ---- Tenant's own address (optional; upserted onto Tenant.AddressId) ----
    public AddressInput? Address { get; set; }
}

public sealed class UpdateTenantStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed record TenantResponse(Guid TenantId, string Identifier, string Status);

/// <summary>
/// Create-only response: also carries the minted default Administrator's user id and one-time temporary
/// password (never retrievable again — same contract as <c>CreateUserResponse</c>).
/// </summary>
public sealed record CreateTenantResponse(
    Guid TenantId, string Identifier, string Status, Guid AdminUserId, string TemporaryPassword);

/// <summary>
/// Response from sending the tenant's default Administrator their login credentials: the temporary
/// password (never retrievable again after this call — same contract as <c>ResetPasswordResponse</c>),
/// whether an email send will be attempted, and whether a brand new password had to be minted (nothing
/// was saved to resend — an older account, or one whose admin already signed in and set their own).
/// </summary>
public sealed record SendTenantCredentialsResponse(
    Guid TenantId, Guid UserId, string TemporaryPassword, bool EmailSent, bool PasswordWasReset);

public sealed record TenantSummary(
    Guid TenantId,
    string Name,
    string Identifier,
    string Status,
    string TimeZoneId,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime CreatedOnUtc,
    DateTime UpdatedOnUtc);

/// <summary>One tenant, as its detail page reads it.</summary>
public sealed record TenantDetail(
    Guid TenantId,
    string Name,
    string Identifier,
    string Status,
    string TimeZoneId,
    AddressResponse? Address,
    RecordAudit Audit);
