namespace E_Commerce.Infrastructure.Security.Verification;

/// <summary>
/// Configuration for the verification code service.
/// </summary>
public sealed class VerificationOptions
{
    public const string SectionName = "Verification";

    /// <summary>
    /// Base64‑encoded HMAC secret key used to hash verification codes.
    /// Must be a cryptographically random value of at least 128 bits.
    /// </summary>
    public string HmacSecretKey { get; init; } = string.Empty;
}