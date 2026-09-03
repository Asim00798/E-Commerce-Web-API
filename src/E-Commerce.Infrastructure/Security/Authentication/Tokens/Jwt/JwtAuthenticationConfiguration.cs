using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce.Infrastructure.Security.Authentication.Tokens.Jwt;

/// <summary>
/// Creates <see cref="TokenValidationParameters"/> for the JWT bearer configuration.
/// </summary>
internal static class JwtAuthenticationConfiguration
{
    public static TokenValidationParameters Create(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer))
            throw new InvalidOperationException("JWT issuer is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("JWT audience is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Secret))
            throw new InvalidOperationException("JWT secret is not configured.");

        var secretBytes = Encoding.UTF8.GetBytes(settings.Secret);
        if (secretBytes.Length < 32)
            throw new InvalidOperationException("JWT secret must contain at least 32 bytes.");

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
            ValidateLifetime = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
}