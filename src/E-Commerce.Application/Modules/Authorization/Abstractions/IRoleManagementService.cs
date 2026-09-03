using E_Commerce.Application.Modules.Authorization.Dtos;

namespace E_Commerce.Application.Modules.Authorization.Abstractions;

/// <summary>
/// Application‑level service for managing roles and user‑role assignments.
/// </summary>
public interface IRoleManagementService
{
    /// <summary>
    /// Creates a new role with the given name.
    /// Returns the new role ID.
    /// </summary>
    Task<Guid> CreateRoleAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the name of an existing role.
    /// </summary>
    Task UpdateRoleAsync(
        Guid roleId,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by ID.
    /// </summary>
    Task DeleteRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task AssignRoleToUserAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task RemoveRoleFromUserAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a role by ID.
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all roles in the system.
    /// </summary>
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(
        CancellationToken cancellationToken = default);
}