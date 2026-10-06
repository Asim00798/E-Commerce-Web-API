using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;

public sealed class PaymobStatusResponse
{
    [JsonPropertyName("id")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("intention_id")]
    public string? IntentionId { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    [JsonPropertyName("error_occured")]           // Paymob's spelling
    public bool ErrorOccurred { get; init; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; init; }

    [JsonPropertyName("refunded_amount_cents")]
    public long? RefundedAmountCents { get; init; }
}