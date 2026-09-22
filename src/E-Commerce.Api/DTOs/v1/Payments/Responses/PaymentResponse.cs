namespace E_Commerce.Api.DTOs.v1.Payments.Responses;

public sealed class PaymentResponse
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? ProviderIntentionId { get; init; }
    public string? ProviderTransactionId { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public decimal RefundedAmount { get; init; }
}