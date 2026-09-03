namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for the Finance bounded context.
/// These must match the permission names stored in the database.
/// </summary>
public static class FinancePermissions
{
    public const string Read = "Payments.Read";
    public const string Refund = "Payments.Refund";
    public const string Manage = "Payments.Manage";
}