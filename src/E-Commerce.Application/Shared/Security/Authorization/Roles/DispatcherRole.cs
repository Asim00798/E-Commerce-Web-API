using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class DispatcherRole : IRole
{
    public string Name => SystemRoles.Dispatcher;

    public IEnumerable<string> GetPermissions() =>
    [
        ShippingPermissions.Read,
        ShippingPermissions.Assign,
        ShippingPermissions.Manage
    ];
}