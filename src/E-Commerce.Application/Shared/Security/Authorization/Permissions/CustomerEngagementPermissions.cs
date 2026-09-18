using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for customer engagement operations.
/// </summary>
public sealed class CustomerEngagementPermissions : IPermissionSource
{
    public const string Read = "Engagement.Read";
    public const string Rate = "Engagement.Rate";
    public const string Wishlist = "Engagement.Wishlist";
}