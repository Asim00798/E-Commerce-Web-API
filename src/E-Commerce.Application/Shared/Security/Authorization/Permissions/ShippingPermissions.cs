namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for the Shipping bounded context.
/// These must match the permission names stored in the database.
/// Shipment creation is intentionally not included because it is a system/integration operation.
/// </summary>
public static class ShippingPermissions
{
    public const string Read = "Shipments.Read";
    public const string Assign = "Shipments.Assign";
    public const string Manage = "Shipments.Manage";
    public const string Deliver = "Shipments.Deliver";
}