namespace E_Commerce.Application.Shared.Security.Identity;

/// <summary>
/// Application capability for provisioning and existence checks.
/// This interface remains narrowly focused on account creation and lookup,
/// not on lifecycle or credential management.
/// </summary>
public interface IIdentityService
{
    Task<Guid> CreateUserWithPrehashedPasswordAsync(
        CreateIdentityUserRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneAsync(string normalizedPhone, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request used to create a new Identity user with an already‑hashed password.
/// </summary>
public sealed class CreateIdentityUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Username { get; init; }
}