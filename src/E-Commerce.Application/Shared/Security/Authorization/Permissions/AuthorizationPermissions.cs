using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for authorization management operations.
/// </summary>
public sealed class AuthorizationPermissions: IPermissionSource
{
    public const string Manage = "Authorization.Manage";
}