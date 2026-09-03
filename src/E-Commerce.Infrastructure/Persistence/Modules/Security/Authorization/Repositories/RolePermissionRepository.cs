using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Repositories;

/// <summary>
/// Repository for managing <see cref="RolePermission"/> entities and checking role-permission assignments.
/// </summary>
internal sealed class RolePermissionRepository
{
    private readonly AppDbContext _dbContext;

    public RolePermissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Checks whether any of the given role IDs have the specified permission.
    /// </summary>
    public async Task<bool> RolesHavePermissionAsync(
        IReadOnlyCollection<Guid> roleIds,
        string permissionName,
        CancellationToken ct = default)
    {
        if (roleIds.Count == 0)
            return false;

        return await _dbContext.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Permission.Name == permissionName, ct);
    }

    public async Task<List<RolePermission>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken ct = default)
        => await _dbContext.Set<RolePermission>()
            .Include(rp => rp.Permission)   
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken ct = default)
        => await _dbContext.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, ct);

    public async Task AddAsync(RolePermission rolePermission, CancellationToken ct = default)
        => await _dbContext.Set<RolePermission>().AddAsync(rolePermission, ct);

    public void Remove(RolePermission rolePermission)
        => _dbContext.Set<RolePermission>().Remove(rolePermission);
}