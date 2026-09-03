using E_Commerce.Application.Shared.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Infrastructure.Security.Cryptography;

/// <summary>
/// Hashes opaque refresh tokens using HMACSHA256 with a secret key,
/// so that only the hash is stored in the database.
/// </summary>
internal sealed class RefreshTokenHasher 
{
    private readonly byte[] _secretKey;

    public RefreshTokenHasher(IOptions<RefreshTokenHasherOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.SecretKey))
            throw new ArgumentException("Refresh token hashing secret key must be configured.");

        _secretKey = Convert.FromBase64String(options.Value.SecretKey);
    }

    /// <inheritdoc />
    public string Hash(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new ArgumentException("Raw token cannot be null or empty.", nameof(rawToken));

        using var hmac = new HMACSHA256(_secretKey);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc />
    public bool Verify(string rawToken, string storedHash)
    {
        if (storedHash is null) return false;

        var computedHash = Hash(rawToken);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(storedHash));
    }
}