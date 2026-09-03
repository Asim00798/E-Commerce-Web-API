using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Security.Identity.Services;

/// <summary>
/// Implements <see cref="IAccountReader"/> to query account information
/// without exposing Identity internals.
/// </summary>
internal sealed class AccountReader : IAccountReader
{
    private readonly UserManager<User> _userManager;

    public AccountReader(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    /// <inheritdoc />
    public async Task<AccountDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToAccountDto(user);
    }

    /// <inheritdoc />
    public async Task<AccountDto?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        // Normalization is an Identity concern and belongs here, not in Application.
        var normalizedEmail = _userManager.NormalizeEmail(email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        return user is null ? null : ToAccountDto(user);
    }

    /// <inheritdoc />
    public async Task<AccountSecurityDto?> GetSecurityAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var logins = await _userManager.GetLoginsAsync(user);
        var isLockedOut = await _userManager.IsLockedOutAsync(user);

        return new AccountSecurityDto
        {
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsLockedOut = isLockedOut,
            LockoutEnd = user.LockoutEnd,
            HasPassword = await _userManager.HasPasswordAsync(user),
            ExternalLoginProviders = logins.Select(l => l.LoginProvider).ToList(),
            AccountStatus = user.AccountStatus,
            PasswordLastChangedAtUtc = user.PasswordLastChangedAtUtc
        };
    }

    private static AccountDto ToAccountDto(User user) => new()
    {
        UserId = user.Id,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        CreatedAtUtc = user.CreatedAtUtc,
        AccountStatus = user.AccountStatus,
        EmailConfirmed = user.EmailConfirmed,
        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        TwoFactorEnabled = user.TwoFactorEnabled
    };
}