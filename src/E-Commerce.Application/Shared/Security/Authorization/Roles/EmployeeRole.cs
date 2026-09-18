using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class EmployeeRole : IRole
{
    public string Name => SystemRoles.Employee;

    public IEnumerable<string> GetPermissions() =>
    [
        ShippingPermissions.Read,
        OrderingPermissions.Read,
        PeoplePermissions.ReadOwn
    ];
}