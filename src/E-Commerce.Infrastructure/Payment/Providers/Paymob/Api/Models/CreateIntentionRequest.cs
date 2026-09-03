using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;

public sealed class CreateIntentionRequest
{
    [JsonPropertyName("amount")]
    public long AmountInMinorUnit { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("integration_id")]
    public int IntegrationId { get; init; }

    [JsonPropertyName("merchant_order_id")]
    public string MerchantOrderId { get; init; } = string.Empty;

    [JsonPropertyName("return_url")]
    public string ReturnUrl { get; init; } = string.Empty;

    [JsonPropertyName("cancel_url")]
    public string CancelUrl { get; init; } = string.Empty;

    [JsonPropertyName("idempotency_key")]
    public string IdempotencyKey { get; init; } = string.Empty;
}