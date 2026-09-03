namespace E_Commerce.Application.Shared.Security.Authorization.Services;

/// <summary>
/// Application‑level service used to check whether a user is in a specific role.
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Returns true if the user is a member of the given role.
    /// </summary>
    Task<bool> HasRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);
}