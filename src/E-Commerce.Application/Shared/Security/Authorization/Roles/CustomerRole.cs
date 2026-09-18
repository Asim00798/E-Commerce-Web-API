using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class CustomerRole : IRole
{
    public string Name => SystemRoles.Customer;

    public IEnumerable<string> GetPermissions() =>
    [
        PeoplePermissions.ReadOwn,
        PeoplePermissions.CreateOwn,
        PeoplePermissions.UpdateOwn,
        PeoplePermissions.DeleteOwn,
        OrderingPermissions.Read,
        OrderingPermissions.Place,
        OrderingPermissions.Cancel,
        CustomerEngagementPermissions.Read,
        CustomerEngagementPermissions.Rate,
        CustomerEngagementPermissions.Wishlist
    ];
}