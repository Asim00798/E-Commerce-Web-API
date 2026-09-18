using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for the Finance bounded context.
/// These must match the permission names stored in the database.
/// </summary>
public sealed class FinancePermissions : IPermissionSource
{
    public const string Read = "Payments.Read";
    public const string Refund = "Payments.Refund";
    public const string Manage = "Payments.Manage";
}