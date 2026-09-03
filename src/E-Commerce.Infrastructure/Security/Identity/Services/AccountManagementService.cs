using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Identity.Services;

/// <summary>
/// Implements <see cref="IAccountManagement"/> using ASP.NET Core Identity.
/// Manages application‑level account lifecycle (activation/deactivation)
/// and administrative lockout operations.
/// </summary>
internal sealed class AccountManagementService : IAccountManagement
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AccountManagementService> _logger;

    public AccountManagementService(
        UserManager<User> userManager,
        AppDbContext dbContext,
        ILogger<AccountManagementService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ActivateAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        if (user.AccountStatus == AccountStatus.Active)
            return;

        user.AccountStatus = AccountStatus.Active;
        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result, "Account activation failed", userId);

        _logger.LogInformation("User {UserId} activated.", userId);
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        if (user.AccountStatus == AccountStatus.Deactivated)
            return;

        user.AccountStatus = AccountStatus.Deactivated;
        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result, "Account deactivation failed", userId);

        _logger.LogInformation("User {UserId} deactivated.", userId);
    }

    public async Task LockAsync(Guid userId, DateTimeOffset? lockoutEnd, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        var effectiveLockoutEnd = lockoutEnd ?? DateTimeOffset.MaxValue;
        var result = await _userManager.SetLockoutEndDateAsync(user, effectiveLockoutEnd);
        EnsureSuccess(result, "Account lock failed", userId);

        _logger.LogInformation("User {UserId} locked until {LockoutEnd}.", userId, effectiveLockoutEnd);
    }

    public async Task UnlockAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
        EnsureSuccess(unlockResult, "Account unlock failed", userId);

        var resetResult = await _userManager.ResetAccessFailedCountAsync(user);
        EnsureSuccess(resetResult, "Resetting access failed count failed", userId);

        _logger.LogInformation("User {UserId} unlocked and access failed count reset.", userId);
    }

    public async Task UpdateAsync(UpdateAccountRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {request.UserId} not found.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var result = await _userManager.SetEmailAsync(user, request.Email);
            EnsureSuccess(result, "Email update failed", request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var result = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            EnsureSuccess(result, "Phone number update failed", request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var result = await _userManager.SetUserNameAsync(user, request.UserName);
            EnsureSuccess(result, "Username update failed", request.UserId);
        }

        _logger.LogInformation("User {UserId} updated.", request.UserId);
    }

    private void EnsureSuccess(IdentityResult result, string action, Guid userId)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogError("{Action} for user {UserId}: {Errors}", action, userId, errors);
            throw new IdentityOperationException($"{action}: {errors}");
        }
    }
}