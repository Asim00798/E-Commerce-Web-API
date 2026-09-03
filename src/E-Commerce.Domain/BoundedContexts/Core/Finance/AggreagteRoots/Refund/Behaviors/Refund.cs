using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Behaviors;

public sealed class Refund : BaseEntity, IAggregateRoot
{
    public Guid PaymentId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public RefundStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? ProviderTransactionId { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Refund()
    {
        // EF Core
    }

    private Refund(
        Guid paymentId,
        Money amount,
        string reason)
    {
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Requested;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public static Refund Create(
        Guid paymentId,
        Money amount,
        string reason)
    {
        if (amount.Amount <= 0)
            throw new RefundException("Refund amount must be greater than zero.");

        return new Refund(paymentId, amount, reason);
    }

    public void MarkProcessing()
    {
        if (Status != RefundStatus.Requested)
            throw new RefundException("Refund can only be marked processing from requested state.");

        Status = RefundStatus.Processing;
    }

    public void Complete()
    {
        if (Status != RefundStatus.Processing)
            throw new RefundException("Refund can only be completed from processing state.");

        Status = RefundStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new RefundCompletedDomainEvent(
            Id,
            PaymentId,
            Amount));
    }

    public void Fail(string? reason = null)
    {
        if (Status != RefundStatus.Processing)
            throw new RefundException("Refund can only be failed from processing state.");

        Status = RefundStatus.Failed;

        AddDomainEvent(new RefundFailedDomainEvent(
            Id,
            PaymentId,
            Amount,
            reason));
    }

    /// <summary>
    /// Recovers a stuck Processing refund back to Requested so it can be retried.
    /// This is a legitimate recovery transition, not a business outcome.
    /// </summary>
    public void Requeue()
    {
        if (Status != RefundStatus.Processing)
            throw new RefundException("Only a processing refund can be requeued.");

        Status = RefundStatus.Requested;
    }

    public void SetProviderTransactionId(string providerTransactionId)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
            throw new RefundException("Provider transaction ID is required.");

        ProviderTransactionId = providerTransactionId;
    }

}