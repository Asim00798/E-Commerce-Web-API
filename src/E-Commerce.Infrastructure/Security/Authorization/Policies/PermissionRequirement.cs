using Microsoft.AspNetCore.Authorization;

namespace E_Commerce.Infrastructure.Security.Authorization.Policies;

/// <summary>
/// Represents the requirement that the current user must have a specific permission.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be null or empty.", nameof(permission));
        Permission = permission;
    }
}