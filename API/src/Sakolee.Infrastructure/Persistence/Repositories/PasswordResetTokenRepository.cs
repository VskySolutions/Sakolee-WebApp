using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sakolee.Infrastructure.Persistence.Repositories;

internal sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly SakoleeDbContext _dbContext;

    public PasswordResetTokenRepository(SakoleeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _dbContext.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => await _dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);

    public void Update(PasswordResetToken token) => _dbContext.PasswordResetTokens.Update(token);

    public async Task InvalidateAllForUserAsync(Guid userId, DateTime onUtc, CancellationToken cancellationToken = default)
    {
        var live = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedOnUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var token in live)
        {
            token.UsedOnUtc = onUtc;
        }
    }
}
