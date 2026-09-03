using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Identity.Services;

/// <summary>
/// Implements <see cref="ICredentialManagement"/> using ASP.NET Core Identity.
/// Handles password change, reset token generation, and password reset.
/// Password update and metadata timestamp are committed atomically.
/// </summary>
internal sealed class CredentialManagementService : ICredentialManagement
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CredentialManagementService> _logger;

    public CredentialManagementService(
        UserManager<User> userManager,
        AppDbContext dbContext,
        ILogger<CredentialManagementService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            EnsureSuccess(result, "Password change failed", userId);

            user.PasswordLastChangedAtUtc = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            EnsureSuccess(updateResult, "Updating password timestamp failed", userId);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        _logger.LogInformation("Password changed for user {UserId}.", userId);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // The token itself is never logged.
        _logger.LogInformation("Password reset token generated for user {UserId}.", userId);
        return token;
    }

    public async Task ResetPasswordAsync(Guid userId, string resetToken, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new IdentityOperationException($"User {userId} not found.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            EnsureSuccess(result, "Password reset failed", userId);

            user.PasswordLastChangedAtUtc = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            EnsureSuccess(updateResult, "Updating password timestamp failed", userId);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        _logger.LogInformation("Password reset for user {UserId} succeeded.", userId);
    }

    private void EnsureSuccess(IdentityResult result, string action, Guid userId)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("{Action} for user {UserId}: {Errors}", action, userId, errors);
            throw new IdentityOperationException($"{action}: {errors}");
        }
    }
}