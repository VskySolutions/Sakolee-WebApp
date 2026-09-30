using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Security;

/// <summary>
/// Emails a login account its credentials (username = login email, plus temporary password) using the
/// "User Invitation" template. Shared by every "Send Credentials" action (Staff list, Family list, …).
/// </summary>
public interface IUserCredentialsService
{
    /// <summary>
    /// Re-sends the user's still-unused temporary password when one is on file
    /// (<see cref="User.EncryptedTemporaryPassword"/>); otherwise mints a fresh one — forcing a change on
    /// next login and ending existing sessions, same as an admin password reset. Persists the change and
    /// then sends synchronously through <paramref name="tenantId"/>'s active SMTP account. The caller is
    /// responsible for authorising access to <paramref name="user"/>.
    /// </summary>
    Task<UserCredentialsResult> SendAsync(User user, Guid? tenantId, CancellationToken cancellationToken = default);
}

/// <param name="TemporaryPassword">The plaintext temporary password — returned so the caller can show it
/// once when the email could not be sent.</param>
/// <param name="EmailSent">Whether the SMTP send actually succeeded.</param>
/// <param name="PasswordWasReset">Whether a new password had to be minted (none was on file to resend).</param>
public sealed record UserCredentialsResult(string TemporaryPassword, bool EmailSent, bool PasswordWasReset);
