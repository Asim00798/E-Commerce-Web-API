namespace E_Commerce.Application.Modules.Authentication.Dtos;

/// <summary>
/// Pair of access and refresh tokens returned after successful authentication.
/// </summary>
public sealed class TokenPairDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}