namespace E_Commerce.Api.DTOs.Payments.Responses;

public sealed class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ProviderIntentionId { get; set; }
    public string? ProviderTransactionId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public decimal RefundedAmount { get; set; }
}