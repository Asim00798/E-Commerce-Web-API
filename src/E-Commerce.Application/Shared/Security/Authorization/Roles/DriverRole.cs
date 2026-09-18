using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class DriverRole : IRole
{
    public string Name => SystemRoles.Driver;

    public IEnumerable<string> GetPermissions() =>
    [
        ShippingPermissions.Deliver
    ];
}