using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce.Infrastructure.Security.Authentication.Tokens.Jwt;

/// <summary>
/// Generates short-lived JWT access tokens.
/// </summary>
internal sealed class JwtTokenGenerator 
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Generates an access token and returns the token string together with its expiry.
    /// </summary>
    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(
        IEnumerable<Claim> claims,
        DateTime nowUtc)
    {
        var expiresAtUtc = nowUtc.AddMinutes(_settings.ExpiryMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return (serialized, expiresAtUtc);
    }
}