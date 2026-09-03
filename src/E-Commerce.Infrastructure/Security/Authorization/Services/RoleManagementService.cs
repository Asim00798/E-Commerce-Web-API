using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Authorization.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Security.Authorization.Services;

/// <summary>
/// Implements role management using ASP.NET Core Identity built-in roles.
/// </summary>
internal sealed class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<Security.Identity.Entities.User> _userManager;

    public RoleManagementService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<Security.Identity.Entities.User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Guid> CreateRoleAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        var role = new IdentityRole<Guid> { Name = name };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return role.Id;
    }

    public async Task UpdateRoleAsync(Guid roleId, string name, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new InvalidOperationException("Role not found.");

        role.Name = name;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
            return;

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task AssignRoleToUserAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
            return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        };
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _roleManager.Roles.ToListAsync(ct);
        return roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name ?? string.Empty }).ToList();
    }
}