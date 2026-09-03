namespace E_Commerce.Application.Shared.Security.Identity;

/// <summary>
/// Application capability for credential operations:
/// password change, password reset token generation, and password reset.
/// </summary>
public interface ICredentialManagement
{
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<string> GeneratePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid userId, string resetToken, string newPassword, CancellationToken cancellationToken = default);
}