using E_Commerce.Application.Modules.Authorization.Dtos;

namespace E_Commerce.Application.Modules.Authorization.Abstractions;

/// <summary>
/// Application‑level service for managing permissions and role‑permission assignments.
/// </summary>
public interface IPermissionManagementService
{
    /// <summary>
    /// Creates a new permission with the given name and optional description.
    /// Returns the new permission ID.
    /// </summary>
    Task<Guid> CreatePermissionAsync(
        string name,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    Task UpdatePermissionAsync(
        Guid permissionId,
        string name,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission by ID.
    /// </summary>
    Task DeletePermissionAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a permission to a role.
    /// </summary>
    Task AssignPermissionToRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a permission from a role.
    /// </summary>
    Task RemovePermissionFromRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a permission by ID.
    /// </summary>
    Task<PermissionDto?> GetPermissionByIdAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all permissions in the system.
    /// </summary>
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all permissions assigned to the specified role.
    /// </summary>
    Task<IReadOnlyList<PermissionDto>> GetPermissionsForRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}