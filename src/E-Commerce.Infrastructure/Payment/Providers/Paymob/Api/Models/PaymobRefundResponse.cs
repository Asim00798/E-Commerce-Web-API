using System.Text.Json.Serialization;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;

/// <summary>
/// Response from POST /api/acceptance/void_refund/refund.
/// The response is a refund transaction object.
/// </summary>
public sealed class PaymobRefundResponse
{
    [JsonPropertyName("id")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("pending")]
    public bool Pending { get; init; }

    [JsonPropertyName("error_occured")]           // Paymob's spelling
    public bool ErrorOccurred { get; init; }

    [JsonPropertyName("is_refund")]
    public bool IsRefund { get; init; }

    [JsonPropertyName("parent_transaction")]
    public string? ParentTransaction { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}