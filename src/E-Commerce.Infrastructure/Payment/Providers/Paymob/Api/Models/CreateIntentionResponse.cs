using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;

public sealed class CreateIntentionResponse
{
    [JsonPropertyName("id")]
    public string IntentionId { get; init; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; init; }

    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; init; }
}