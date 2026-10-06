using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

/// <summary>
/// Verifies Paymob transaction callback HMAC signatures.
///
/// The canonical string is the concatenation of specific fields in a
/// documented order, with no separator. Paymob's field list has changed
/// across API versions — verify against the current Intention API
/// documentation before deployment.
/// </summary>
public sealed class PaymobHmacVerifier
{
    private readonly string _webhookSecret;

    public PaymobHmacVerifier(string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(webhookSecret))
            throw new ArgumentException(
                "Paymob webhook secret is required.", nameof(webhookSecret));

        _webhookSecret = webhookSecret;
    }

    public bool Verify(TransactionCallback callback, string receivedHmac)
    {
        if (callback is null || string.IsNullOrWhiteSpace(receivedHmac))
            return false;

        var canonical = BuildCanonicalString(callback);

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_webhookSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        var expected = Convert.ToHexString(hashBytes).ToLowerInvariant();

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedHmac.ToLowerInvariant());

        // FixedTimeEquals requires equal-length inputs.
        if (expectedBytes.Length != receivedBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }

    private static string BuildCanonicalString(TransactionCallback callback)
    {
        // Concatenate in Paymob's documented order, no separator.
        // Verify against current Paymob documentation before deployment.
        return string.Concat(
            LongToString(callback.AmountCents),
            callback.CreatedAt ?? string.Empty,
            callback.Currency ?? string.Empty,
            BoolToString(callback.ErrorOccurred),
            BoolToString(callback.HasParentTransaction),
            LongToString(callback.Id),
            LongToString(callback.IntegrationId),
            BoolToString(callback.Is3dSecure),
            BoolToString(callback.IsAuth),
            BoolToString(callback.IsCapture),
            BoolToString(callback.IsRefunded),
            BoolToString(callback.IsStandalonePayment),
            BoolToString(callback.IsVoided),
            LongToString(callback.Order?.Id),
            LongToString(callback.Owner),
            BoolToString(callback.Pending),
            callback.SourceData?.Pan ?? string.Empty,
            callback.SourceData?.SubType ?? string.Empty,
            callback.SourceData?.Type ?? string.Empty,
            BoolToString(callback.Success));
    }

    private static string BoolToString(bool value) => value ? "true" : "false";

    private static string LongToString(long? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}