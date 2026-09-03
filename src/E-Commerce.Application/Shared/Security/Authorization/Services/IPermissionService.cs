namespace E_Commerce.Application.Shared.Security.Authorization.Services;

/// <summary>
/// Application‑level service used to check whether a user has a specific permission.
/// Permissions are granted through roles.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Returns true if the specified user has the requested permission.
    /// </summary>
    Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);
}