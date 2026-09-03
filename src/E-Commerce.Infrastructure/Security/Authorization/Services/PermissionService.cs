using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Repositories;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Security.Authorization.Services;

/// <summary>
/// Implements <see cref="IPermissionService"/> by checking the user's roles and
/// the permissions assigned to those roles.
/// </summary>
internal sealed class PermissionService : IPermissionService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly RolePermissionRepository _rolePermissionRepository;

    public PermissionService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        RolePermissionRepository rolePermissionRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _rolePermissionRepository = rolePermissionRepository;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        var roleNames = await _userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
            return false;

        // Resolve role IDs from role names
        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
            return false;

        return await _rolePermissionRepository.RolesHavePermissionAsync(
            roleIds,
            permission,
            cancellationToken);
    }
}