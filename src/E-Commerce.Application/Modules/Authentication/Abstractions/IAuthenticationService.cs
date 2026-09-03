using E_Commerce.Application.Modules.Authentication.Dtos;

namespace E_Commerce.Application.Modules.Authentication.Abstractions;

/// <summary>
/// Application capability for authentication use cases:
/// local login, refresh token, logout, external provider authentication, and Google linking.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<AuthenticationResultDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a refresh token and issues a new token pair.
    /// </summary>
    Task<AuthenticationResultDto> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user through an external provider using the provider's subject identifier.
    /// </summary>
    Task<AuthenticationResultDto> ExternalAuthenticateAsync(
        string provider,
        string subjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links the current user to a Google external login using the validated Google subject identifier.
    /// </summary>
    Task LinkGoogleAsync(
        Guid userId,
        string subjectId,
        CancellationToken cancellationToken = default);
}