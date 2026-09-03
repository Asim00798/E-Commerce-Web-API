using System.Security.Cryptography;
using System.Text;
using E_Commerce.Application.Shared.Security.Verification;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Security.Verification;

/// <summary>
/// HMAC‑based implementation of <see cref="IVerificationCodeService"/>.
/// Generates 6‑digit random codes and verifies them using constant‑time comparison.
/// </summary>
internal sealed class VerificationCodeService : IVerificationCodeService
{
    private readonly byte[] _secretKey;

    public VerificationCodeService(IOptions<VerificationOptions> options)
    {
        _secretKey = Convert.FromBase64String(options.Value.HmacSecretKey);
    }

    /// <inheritdoc />
    public string GenerateCode(out string plainCode)
    {
        plainCode = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString();
        return HashWithSecret(plainCode);
    }

    /// <inheritdoc />
    public bool VerifyCode(string submittedCode, string storedHash)
    {
        if (storedHash is null)
            return false;

        var computedHash = HashWithSecret(submittedCode);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(storedHash));
    }

    private string HashWithSecret(string code)
    {
        using var hmac = new HMACSHA256(_secretKey);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}