using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Email;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;

namespace Sakolee.Application.Security;

/// <inheritdoc cref="IUserCredentialsService"/>
internal sealed class UserCredentialsService : IUserCredentialsService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICredentialEncryptionService _credentialEncryption;
    private readonly IEmailNotificationService _emailNotifications;
    private readonly IAuditTrailService _audit;
    private readonly IUnitOfWork _unitOfWork;

    public UserCredentialsService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        ICredentialEncryptionService credentialEncryption,
        IEmailNotificationService emailNotifications,
        IAuditTrailService audit,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _credentialEncryption = credentialEncryption;
        _emailNotifications = emailNotifications;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserCredentialsResult> SendAsync(User user, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        string temporaryPassword;
        var passwordWasReset = string.IsNullOrEmpty(user.EncryptedTemporaryPassword);
        if (passwordWasReset)
        {
            // Nothing saved to resend — mint a fresh one, same as Reset Password.
            temporaryPassword = _passwordHasher.GenerateTemporaryPassword();
            var (hash, salt) = _passwordHasher.Hash(temporaryPassword);
            user.PasswordHash = hash;
            user.Salt = salt;
            user.MustChangePassword = true;
            user.TokenVersion++; // invalidate all existing sessions
            user.EncryptedTemporaryPassword = _credentialEncryption.Encrypt(temporaryPassword);
            _users.Update(user);
            await _refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        }
        else
        {
            temporaryPassword = _credentialEncryption.Decrypt(user.EncryptedTemporaryPassword!);
        }

        await _audit.AddAsync(nameof(User), user.Id.ToString(),
            passwordWasReset ? "CredentialsSent (new password)" : "CredentialsResent", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Sent directly (not via the Hangfire dispatcher) so the admin learns whether the email actually went
        // out. SendAsync never throws — it logs and returns false on no SMTP account / send failure.
        var emailSent = false;
        if (tenantId is { } sendTenant)
        {
            emailSent = await _emailNotifications.SendAsync(sendTenant, EmailTemplateKey.UserInvitation, user.Email,
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["FullName"] = user.DisplayName,
                    ["Email"] = user.Email,
                    ["TemporaryPassword"] = temporaryPassword,
                },
                cancellationToken: cancellationToken);
        }

        return new UserCredentialsResult(temporaryPassword, emailSent, passwordWasReset);
    }
}
