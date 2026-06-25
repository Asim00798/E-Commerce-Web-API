namespace E_Commerce.Application.Shared.Identity;

public interface IIdentityService
{
    // User creation
    Task<Guid> CreateUserAsync(
        string userName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    // Queries
    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default);

    Task<UserInfo?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // Role management
    Task AssignRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // Account control
    Task LockUserAsync(
        Guid userId,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default);

    Task UnlockUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    // Optional permission check (only if you use claims/permissions)
    Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default);
}

// Lightweight DTO returned to Application layer (NEVER IdentityUser)
public sealed record UserInfo(
    Guid Id,
    string UserName,
    string Email,
    bool EmailConfirmed);