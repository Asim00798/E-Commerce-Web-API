namespace E_Commerce.Application.Shared.Security.Verification;

/// <summary>
/// Generates and verifies HMAC‑based verification codes.
/// </summary>
public interface IVerificationCodeService
{
    /// <summary>
    /// Generates a new random code and returns both the plain code and its hash.
    /// </summary>
    string GenerateCode(out string plainCode);

    /// <summary>
    /// Verifies that a submitted code matches a stored hash using constant‑time comparison.
    /// </summary>
    bool VerifyCode(string submittedCode, string storedHash);
}