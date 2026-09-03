namespace E_Commerce.Infrastructure.Security.Authorization.Entities;

/// <summary>
/// Maps a role to a permission. Permissions are granted only through roles.
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}