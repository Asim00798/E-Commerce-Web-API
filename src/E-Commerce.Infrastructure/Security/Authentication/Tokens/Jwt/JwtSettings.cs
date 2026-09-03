namespace E_Commerce.Infrastructure.Security.Authentication.Tokens.Jwt;

/// <summary>
/// Configuration for JWT access tokens.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 15;
}