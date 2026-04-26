namespace E_Commerce.Infrastructure.Security.Jwt;

/// <summary>
/// Generates signed JWT access tokens for authenticated users.
/// </summary>
public sealed class JwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(Microsoft.Extensions.Options.IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>Creates and signs a JWT for the given claims.</summary>
    public string GenerateToken(Guid userId, string email, IEnumerable<string> roles)
    {
        // TODO: Build and sign JWT using System.IdentityModel.Tokens.Jwt
        throw new NotImplementedException();
    }
}
