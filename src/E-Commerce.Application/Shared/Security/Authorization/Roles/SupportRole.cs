using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class SupportRole : IRole
{
    public string Name => SystemRoles.Support;

    public IEnumerable<string> GetPermissions() =>
    [
        OrderingPermissions.Read,
        OrderingPermissions.Manage,
        PeoplePermissions.Read,
        FinancePermissions.Read
    ];
}