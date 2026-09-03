using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Security.Authorization.Services;

/// <summary>
/// Implements <see cref="IUserRoleService"/> using ASP.NET Core Identity.
/// </summary>
internal sealed class UserRoleService : IUserRoleService
{
    private readonly UserManager<User> _userManager;

    public UserRoleService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> HasRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        return await _userManager.IsInRoleAsync(user, role);
    }
}