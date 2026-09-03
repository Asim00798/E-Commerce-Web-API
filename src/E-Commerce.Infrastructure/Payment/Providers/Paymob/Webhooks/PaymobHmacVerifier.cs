using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

public sealed class PaymobHmacVerifier
{
    private readonly string _webhookSecret;

    public PaymobHmacVerifier(string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new ArgumentException("Paymob webhook secret is required.", nameof(webhookSecret));
        }

        _webhookSecret = webhookSecret;
    }

    public bool Verify(TransactionCallback callback, string receivedHmac)
    {
        if (callback is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(receivedHmac))
        {
            return false;
        }

        var canonical = BuildCanonicalString(callback);

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_webhookSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        var expected = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(receivedHmac.ToLowerInvariant()));
    }

    private static string BuildCanonicalString(TransactionCallback callback)
    {
        // Paymob transaction callback HMAC is computed over these fields in this exact order.
        // Verify against current Paymob documentation if the API version changes.
        return string.Join(
            string.Empty,
            callback.AmountInMinorUnit?.ToString() ?? string.Empty,
            callback.Currency ?? string.Empty,
            callback.TransactionId ?? string.Empty,
            callback.IntentionId ?? string.Empty,
            callback.Success ? "true" : "false",
            callback.Pending ? "true" : "false",
            callback.ErrorOccurred ? "true" : "false"
        );
    }
}