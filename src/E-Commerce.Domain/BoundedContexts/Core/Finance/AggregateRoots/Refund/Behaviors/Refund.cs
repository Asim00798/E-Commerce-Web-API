using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;

public sealed partial class Refund : BaseEntity, IAggregateRoot
{
    public Guid PaymentId { get; private set; }
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public RefundStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? ProviderTransactionId { get; private set; }

    private Refund()
    {
        // EF Core
    }

    private Refund(
        Guid paymentId,
        Guid orderId,
        Money amount,
        string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Requested;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public static Refund Create(
        Guid paymentId,
        Guid orderId,
        Money amount,
        string reason)
    {
        if (amount.Amount <= 0)
            throw new RefundException("Refund amount must be greater than zero.");

        return new Refund(paymentId, orderId, amount, reason);
    }
}