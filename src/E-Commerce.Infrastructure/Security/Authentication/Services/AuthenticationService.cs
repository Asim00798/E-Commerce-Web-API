using System.Security.Claims;
using System.Security.Cryptography;
using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.SharedKernel.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Security.Authentication.Repositories;
using E_Commerce.Infrastructure.Security.Authentication.Tokens.Jwt;
using E_Commerce.Infrastructure.Security.Authentication.Tokens.Refresh;
using E_Commerce.Infrastructure.Security.Cryptography;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Security.Authentication.Services;

/// <summary>
/// Orchestrates authentication use cases across local and external login,
/// refresh token rotation, logout, and Google linking.
/// </summary>
internal sealed class AuthenticationService : IAuthenticationService
{
    private const string GoogleProvider = "Google";

    private readonly UserManager<User> _userManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private readonly RefreshTokenHasher _refreshTokenHasher;
    private readonly IClock _clock;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly TimeSpan _refreshTokenLifetime;

    public AuthenticationService(
        UserManager<User> userManager,
        JwtTokenGenerator jwtTokenGenerator,
        RefreshTokenRepository refreshTokenRepository,
        RefreshTokenHasher refreshTokenHasher,
        IClock clock,
        ILogger<AuthenticationService> logger,
        IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenHasher = refreshTokenHasher;
        _clock = clock;
        _logger = logger;
        _refreshTokenLifetime = refreshTokenOptions.Value.TokenLifetime;
    }

    // ========================================================================
    // Public interface methods
    // ========================================================================

    public async Task<AuthenticationResultDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var inputValidation = ValidateLoginInput(email, password);
        if (inputValidation is not null)
            return inputValidation;

        var user = await FindUserByEmailAsync(email, cancellationToken);
        if (user is null)
            return Fail("Invalid email or password.");

        var accountValidation = await ValidateAccountStateAsync(user, cancellationToken);
        if (accountValidation is not null)
            return accountValidation;

        var passwordValidation = await ValidatePasswordAsync(user, password);
        if (passwordValidation is not null)
            return passwordValidation;

        await _userManager.ResetAccessFailedCountAsync(user);

        return await CreateTokenPairAsync(user, cancellationToken);
    }

    public async Task<AuthenticationResultDto> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Fail("Invalid or expired refresh token.");

        var nowUtc = _clock.UtcNow;
        var tokenHash = _refreshTokenHasher.Hash(refreshToken);

        await using var transaction = await _refreshTokenRepository
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var revoked = await _refreshTokenRepository
                .TryRevokeAsync(tokenHash, nowUtc, cancellationToken);

            if (!revoked)
            {
                var existing = await _refreshTokenRepository
                    .GetByHashAsync(tokenHash, cancellationToken);

                if (await RevokeFamilyIfReusedAsync(existing, nowUtc, cancellationToken))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return Fail("Refresh token reuse detected. Please sign in again.");
                }

                await transaction.RollbackAsync(cancellationToken);
                return Fail("Invalid or expired refresh token.");
            }

            var storedToken = await _refreshTokenRepository
                .GetByHashAsync(tokenHash, cancellationToken);

            if (storedToken is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Fail("Invalid or expired refresh token.");
            }

            var user = await _userManager
                .FindByIdAsync(storedToken.UserId.ToString());

            var userValidation = await ValidateRefreshUserAsync(user, cancellationToken);
            if (userValidation is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return userValidation;
            }

            var replacement = CreateRefreshTokenEntity(
                user!,
                nowUtc,
                storedToken.TokenFamilyId);

            storedToken.ReplacedByTokenHash = replacement.TokenHash;

            await _refreshTokenRepository
                .AddAsync(replacement.Entity, cancellationToken);

            await _refreshTokenRepository
                .SaveChangesAsync(cancellationToken);

            // Generate the JWT before committing the transaction.
            var accessToken = _jwtTokenGenerator
                .GenerateAccessToken(BuildClaims(user!), nowUtc);

            await transaction.CommitAsync(cancellationToken);

            return AuthenticationResultDto.Success(
                new TokenPairDto
                {
                    AccessToken = accessToken.Token,
                    RefreshToken = replacement.RawToken,
                    AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                    RefreshTokenExpiresAtUtc = replacement.ExpiresAtUtc
                });
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var tokenHash = _refreshTokenHasher.Hash(refreshToken);
        var storedToken = await _refreshTokenRepository
            .GetByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked)
            return;

        storedToken.IsRevoked = true;
        storedToken.RevokedAtUtc = _clock.UtcNow;

        try
        {
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Already revoked by another request.
        }
    }

    public async Task<AuthenticationResultDto> ExternalAuthenticateAsync(
        string provider,
        string subjectId,
        CancellationToken cancellationToken)
    {
        if (!IsSupportedGoogleProvider(provider, subjectId))
            return Fail("External authentication failed.");

        var user = await _userManager
            .FindByLoginAsync(GoogleProvider, subjectId);

        if (user is null)
        {
            _logger.LogInformation(
                "External authentication failed. No user linked to provider {Provider}.",
                GoogleProvider);

            return Fail("No account linked to this provider.");
        }

        var accountValidation = await ValidateAccountStateAsync(user, cancellationToken);
        if (accountValidation is not null)
            return accountValidation;

        return await CreateTokenPairAsync(user, cancellationToken);
    }

    public async Task LinkGoogleAsync(
        Guid userId,
        string subjectId,
        CancellationToken cancellationToken)
    {
        EnsureGoogleSubject(subjectId);

        var user = await FindUserForLinkingAsync(userId);

        ValidateUserActiveForLinking(user);

        await EnsureGoogleNotLinkedToAnotherUserAsync(user, subjectId);

        await AddGoogleLoginAsync(user, subjectId);
    }

    // ========================================================================
    // Login helpers
    // ========================================================================

    private AuthenticationResultDto? ValidateLoginInput(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Fail("Invalid email or password.");

        return null;
    }

    private async Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = _userManager.NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return null;

        return await _userManager.FindByEmailAsync(normalizedEmail);
    }

    private async Task<AuthenticationResultDto?> ValidateAccountStateAsync(
        User user,
        CancellationToken cancellationToken)
    {
        if (user.AccountStatus != AccountStatus.Active)
            return Fail("Account is deactivated.");

        if (await _userManager.IsLockedOutAsync(user))
            return Fail("Account is locked. Try again later.");

        return null;
    }

    private async Task<AuthenticationResultDto?> ValidatePasswordAsync(
        User user,
        string password)
    {
        if (await _userManager.CheckPasswordAsync(user, password))
            return null;

        await _userManager.AccessFailedAsync(user);

        return Fail("Invalid email or password.");
    }

    // ========================================================================
    // Refresh token helpers
    // ========================================================================

    private async Task<bool> RevokeFamilyIfReusedAsync(
        RefreshToken? existing,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (existing is null ||
            !existing.IsRevoked ||
            existing.ReplacedByTokenHash is null)
        {
            return false;
        }

        await _refreshTokenRepository.RevokeFamilyAsync(
            existing.TokenFamilyId,
            nowUtc,
            cancellationToken);

        _logger.LogWarning(
            "Refresh token reuse detected for family {TokenFamilyId}. Family revoked.",
            existing.TokenFamilyId);

        return true;
    }

    private async Task<AuthenticationResultDto?> ValidateRefreshUserAsync(
        User? user,
        CancellationToken cancellationToken)
    {
        if (user is null)
            return Fail("Account is not active.");

        if (user.AccountStatus != AccountStatus.Active)
            return Fail("Account is not active.");

        if (await _userManager.IsLockedOutAsync(user))
            return Fail("Account is locked.");

        return null;
    }

    // ========================================================================
    // Google linking helpers
    // ========================================================================

    private void EnsureGoogleSubject(string subjectId)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
            throw new IdentityOperationException("Google subject is missing.");
    }

    private async Task<User> FindUserForLinkingAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        return user;
    }

    private void ValidateUserActiveForLinking(User user)
    {
        if (user.AccountStatus != AccountStatus.Active)
            throw new IdentityOperationException("Cannot link Google to a deactivated account.");
    }

    private async Task EnsureGoogleNotLinkedToAnotherUserAsync(
        User user,
        string subjectId)
    {
        var existingUser = await _userManager.FindByLoginAsync(GoogleProvider, subjectId);

        if (existingUser is null)
            return;

        if (existingUser.Id == user.Id)
            return;

        throw new IdentityOperationException(
            "This Google account is already linked to another account.");
    }

    private async Task AddGoogleLoginAsync(User user, string subjectId)
    {
        var loginInfo = new UserLoginInfo(GoogleProvider, subjectId, GoogleProvider);

        try
        {
            var result = await _userManager.AddLoginAsync(user, loginInfo);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Google login linked for user {UserId}.",
                    user.Id);

                return;
            }

            await HandleAddLoginFailureAsync(user, subjectId, result);
        }
        catch (DbUpdateException ex)
        {
            await HandleDbUpdateExceptionAsync(user, subjectId, ex);
        }
    }

    private async Task HandleAddLoginFailureAsync(
        User user,
        string subjectId,
        IdentityResult result)
    {
        var linkedUser = await _userManager.FindByLoginAsync(GoogleProvider, subjectId);

        if (linkedUser is not null)
        {
            if (linkedUser.Id == user.Id)
                return;

            throw new IdentityOperationException(
                "This Google account is already linked to another account.");
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));

        throw new IdentityOperationException($"Google linking failed: {errors}");
    }

    private async Task HandleDbUpdateExceptionAsync(
        User user,
        string subjectId,
        DbUpdateException ex)
    {
        var linkedUser = await _userManager.FindByLoginAsync(GoogleProvider, subjectId);

        if (linkedUser is not null)
        {
            if (linkedUser.Id == user.Id)
                return;

            throw new IdentityOperationException(
                "This Google account is already linked to another account.");
        }

        _logger.LogError(
            ex,
            "Google linking failed for user {UserId}.",
            user.Id);

        throw new IdentityOperationException("Google linking failed.");
    }

    // ========================================================================
    // Token creation helpers
    // ========================================================================

    private async Task<AuthenticationResultDto> CreateTokenPairAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;

        // Generate the JWT first. If generation fails, no orphaned refresh token is persisted.
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            BuildClaims(user),
            nowUtc);

        var refreshToken = CreateRefreshTokenEntity(user, nowUtc);

        await _refreshTokenRepository.AddAsync(refreshToken.Entity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return AuthenticationResultDto.Success(
            new TokenPairDto
            {
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.RawToken,
                AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc
            });
    }

    private (RefreshToken Entity, string RawToken, string TokenHash, DateTime ExpiresAtUtc)
        CreateRefreshTokenEntity(
            User user,
            DateTime nowUtc,
            Guid? tokenFamilyId = null)
    {
        var rawToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var tokenHash = _refreshTokenHasher.Hash(rawToken);
        var expiresAtUtc = nowUtc.Add(_refreshTokenLifetime);
        var familyId = tokenFamilyId ?? Guid.NewGuid();

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            TokenFamilyId = familyId,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = expiresAtUtc,
            IsRevoked = false
        };

        return (entity, rawToken, tokenHash, expiresAtUtc);
    }

    // ========================================================================
    // Shared helpers
    // ========================================================================

    private static bool IsSupportedGoogleProvider(string provider, string subjectId)
    {
        return string.Equals(provider, GoogleProvider, StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(subjectId);
    }

    private static IEnumerable<Claim> BuildClaims(User user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());

        if (!string.IsNullOrWhiteSpace(user.Email))
            yield return new Claim(ClaimTypes.Email, user.Email);

        if (!string.IsNullOrWhiteSpace(user.UserName))
            yield return new Claim(ClaimTypes.Name, user.UserName);
    }

    private static AuthenticationResultDto Fail(string error)
        => AuthenticationResultDto.Failure(error);
}