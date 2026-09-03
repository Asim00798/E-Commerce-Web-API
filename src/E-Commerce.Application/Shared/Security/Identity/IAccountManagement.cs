namespace E_Commerce.Application.Shared.Security.Identity;

/// <summary>
/// Application capability for managing the account lifecycle:
/// activation, deactivation, lockout, and account metadata updates.
/// </summary>
public interface IAccountManagement
{
    Task ActivateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task LockAsync(Guid userId, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default);
    Task UnlockAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Immutable request describing an account metadata update.
/// </summary>
public sealed class UpdateAccountRequest
{
    public Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? UserName { get; init; }
}