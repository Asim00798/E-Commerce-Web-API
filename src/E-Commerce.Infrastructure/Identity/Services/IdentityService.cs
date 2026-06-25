using E_Commerce.Application.Shared.Identity;
using E_Commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Identity.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public IdentityService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Guid> CreateUserAsync(
        string userName,
        string email,
        string password,
        Guid? registrationId = null,
        CancellationToken cancellationToken = default)
    {
        var user = new User(userName, email, registrationId)
        {
            UserName = userName,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    public async Task<bool> ExistsByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByNameAsync(userName) != null;
    }

    public async Task<UserInfo?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return null;

        return new UserInfo(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.EmailConfirmed);
    }

    public async Task AssignRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        if (!await _roleManager.RoleExistsAsync(role))
            throw new InvalidOperationException("Role does not exist.");

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task RemoveRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        await _userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return roles.ToList();
    }

    public async Task LockUserAsync(
        Guid userId,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        await _userManager.SetLockoutEndDateAsync(user, until ?? DateTimeOffset.MaxValue);
    }

    public async Task UnlockUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        await _userManager.SetLockoutEndDateAsync(user, null);
    }

    public async Task ConfirmEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            throw new InvalidOperationException("Email confirmation failed.");
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return false;

        var claims = await _userManager.GetClaimsAsync(user);

        return claims.Any(c =>
            c.Type == "permission" &&
            c.Value == permission);
    }
}