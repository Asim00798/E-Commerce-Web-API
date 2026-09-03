using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Authentication.Tokens.Refresh;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.Infrastructure.Persistence.Modules.Security.Authentication.Repositories;

internal sealed class RefreshTokenRepository
{
    private readonly AppDbContext _dbContext;

    public RefreshTokenRepository(AppDbContext dbContext)
        => _dbContext = dbContext;

    /// <summary>
    /// Atomically revokes a refresh token if it is still active and not expired.
    /// Returns true if exactly one row was updated; false otherwise.
    /// </summary>
    public async Task<bool> TryRevokeAsync(
        string tokenHash,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE security.RefreshTokens
            SET IsRevoked = 1,
                RevokedAtUtc = {nowUtc}
            WHERE TokenHash = {tokenHash}
              AND IsRevoked = 0
              AND ExpiresAtUtc > {nowUtc}
        ", cancellationToken);

        return affected == 1;
    }

    /// <summary>
    /// Revokes all tokens belonging to the same token family
    /// when refresh-token reuse is detected.
    /// </summary>
    public async Task RevokeFamilyAsync(
        Guid tokenFamilyId,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE security.RefreshTokens
            SET IsRevoked = 1,
                RevokedAtUtc = {nowUtc}
            WHERE TokenFamilyId = {tokenFamilyId}
              AND IsRevoked = 0
        ", cancellationToken);
    }

    public async Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
        => await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

    public async Task AddAsync(
        RefreshToken token,
        CancellationToken cancellationToken)
        => await _dbContext.Set<RefreshToken>()
            .AddAsync(token, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken)
        => _dbContext.Database.BeginTransactionAsync(cancellationToken);
}