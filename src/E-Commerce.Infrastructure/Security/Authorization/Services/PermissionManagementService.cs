using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Authorization.Dtos;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;   // IUnitOfWork
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Repositories;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Security.Authorization.Services;

/// <summary>
/// Implements permission management operations.
/// </summary>
internal sealed class PermissionManagementService : IPermissionManagementService
{
    private readonly AppDbContext _dbContext;
    private readonly PermissionRepository _permissionRepository;
    private readonly RolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionManagementService(
        AppDbContext dbContext,
        PermissionRepository permissionRepository,
        RolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreatePermissionAsync(
        string name,
        string? description = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty.", nameof(name));

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        await _permissionRepository.AddAsync(permission, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return permission.Id;
    }

    public async Task UpdatePermissionAsync(
        Guid permissionId,
        string name,
        string? description = null,
        CancellationToken ct = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId, ct)
            ?? throw new InvalidOperationException("Permission not found.");

        permission.Name = name;
        permission.Description = description;
        _permissionRepository.Update(permission);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermissionAsync(Guid permissionId, CancellationToken ct = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId, ct);
        if (permission is null)
            return;

        _permissionRepository.Remove(permission);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task AssignPermissionToRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken ct = default)
    {
        if (await _rolePermissionRepository.ExistsAsync(roleId, permissionId, ct))
            return;

        var rp = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        await _rolePermissionRepository.AddAsync(rp, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemovePermissionFromRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken ct = default)
    {
        var rp = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId, ct);

        if (rp is not null)
        {
            _rolePermissionRepository.Remove(rp);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(
        Guid permissionId,
        CancellationToken ct = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId, ct);
        if (permission is null)
            return null;

        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description
        };
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken ct = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(ct);
        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToList();
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsForRoleAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(roleId, ct);

        return rolePermissions
            .Select(rp => rp.Permission)
            .Where(p => p is not null)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            })
            .ToList();
    }
}