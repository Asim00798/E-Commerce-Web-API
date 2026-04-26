namespace E_Commerce.Infrastructure.Security.Jwt;

/// <summary>
/// Strongly-typed JWT configuration settings.
/// Bind from <c>appsettings.json</c> section <c>"Jwt"</c>.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;

    /// <summary>Token expiry in minutes.</summary>
    public int ExpiryMinutes { get; init; } = 60;
}
