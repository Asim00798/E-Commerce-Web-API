using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Application.BoundedContexts.Finance.Dtos;

public sealed record PaymentDto
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string? ProviderIntentionId { get; init; }
    public string? ProviderTransactionId { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public decimal RefundedAmount { get; init; }
}