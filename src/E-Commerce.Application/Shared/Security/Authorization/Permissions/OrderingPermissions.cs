using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for order management operations.
/// </summary>
public sealed class OrderingPermissions : IPermissionSource
{
    public const string Read = "Orders.Read";
    public const string Place = "Orders.Place";
    public const string Cancel = "Orders.Cancel";
    public const string Manage = "Orders.Manage";
    public const string Deliver = "Orders.Deliver";
    public const string ChangeShippingAddress = "Orders.ChangeShippingAddress";
}