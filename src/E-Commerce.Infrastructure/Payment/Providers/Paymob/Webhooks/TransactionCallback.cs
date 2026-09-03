using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

public sealed class TransactionCallback
{
    [JsonPropertyName("id")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("intention_id")]
    public string? IntentionId { get; init; }

    [JsonPropertyName("amount")]
    public long? AmountInMinorUnit { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    [JsonPropertyName("error_occured")]
    public bool ErrorOccurred { get; init; }

    [JsonPropertyName("hmac")]
    public string? Hmac { get; init; }
}