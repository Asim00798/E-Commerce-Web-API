using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class PackagingRole : IRole
{
    public string Name => SystemRoles.Packaging;

    public IEnumerable<string> GetPermissions() =>
    [
        ShippingPermissions.Manage
    ];
}