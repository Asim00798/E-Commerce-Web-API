namespace E_Commerce.Application.Modules.Authorization.Dtos;

/// <summary>
/// Data transfer object representing the assignment of a permission to a role.
/// </summary>
public sealed class RolePermissionDto
{
    /// <summary>
    /// Role identifier.
    /// </summary>
    public Guid RoleId { get; init; }

    /// <summary>
    /// Permission identifier.
    /// </summary>
    public Guid PermissionId { get; init; }
}