using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;

public sealed class PaymobRefundStatusResponse
{
    [JsonPropertyName("id")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    [JsonPropertyName("error_occured")]
    public bool ErrorOccurred { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}