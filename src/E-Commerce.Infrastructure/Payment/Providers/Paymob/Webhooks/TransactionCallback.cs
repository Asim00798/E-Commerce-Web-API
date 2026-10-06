using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

/// <summary>
/// Paymob transaction callback payload.
///
/// The HMAC is computed over a specific ordered subset of these fields.
/// See <see cref="PaymobHmacVerifier"/> for the canonical string construction.
/// </summary>
public sealed class TransactionCallback
{
    // --- Fields included in the HMAC canonical string ---

    [JsonPropertyName("amount_cents")]
    public long? AmountCents { get; init; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("error_occured")]           // Paymob's spelling
    public bool ErrorOccurred { get; init; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; init; }

    [JsonPropertyName("id")]
    public long? Id { get; init; }

    [JsonPropertyName("integration_id")]
    public long? IntegrationId { get; init; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3dSecure { get; init; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; init; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; init; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; init; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; init; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; init; }

    [JsonPropertyName("order")]
    public TransactionCallbackOrder? Order { get; init; }

    [JsonPropertyName("owner")]
    public long? Owner { get; init; }

    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    [JsonPropertyName("source_data")]
    public TransactionCallbackSourceData? SourceData { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    // --- Fields used for correlation, not HMAC ---

    [JsonPropertyName("intention_id")]
    public string? IntentionId { get; init; }
}

public sealed class TransactionCallbackOrder
{
    [JsonPropertyName("id")]
    public long? Id { get; init; }

    [JsonPropertyName("merchant_order_id")]
    public string? MerchantOrderId { get; init; }
}

public sealed class TransactionCallbackSourceData
{
    [JsonPropertyName("pan")]
    public string? Pan { get; init; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
}