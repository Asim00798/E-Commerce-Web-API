namespace E_Commerce.Infrastructure.Security.Cryptography;

/// <summary>
/// Configuration for the <see cref="RefreshTokenHasher"/>.
/// </summary>
public sealed class RefreshTokenHasherOptions
{
    public const string SectionName = "RefreshTokenHasher";

    /// <summary>
    /// Base64‑encoded secret key used for HMACSHA256 hashing.
    /// Must be a cryptographically random value of at least 256 bits (32 bytes).
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;
}