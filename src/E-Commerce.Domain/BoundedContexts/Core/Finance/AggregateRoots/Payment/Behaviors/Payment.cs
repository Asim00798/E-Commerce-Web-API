using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Finance.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;

public sealed partial class Payment : BaseEntity, IAggregateRoot
{
    private readonly List<PaymentTransaction> _transactions = new();

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }

    public string Provider { get; private set; } = string.Empty;
    public string? ProviderIntentionId { get; private set; }
    public string? ProviderTransactionId { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public Money RefundedAmount { get; private set; } = null!;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    private Payment()
    {
        // EF Core
    }

    private Payment(
        Guid orderId,
        Guid customerId,
        Money amount,
        PaymentMethod method)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        Provider = string.Empty;
        RefundedAmount = new Money(0, amount.Currency);
    }

    public static Payment Create(
        Guid orderId,
        Guid customerId,
        Money amount,
        PaymentMethod method)
    {
        if (amount.Amount <= 0)
            throw new PaymentException("Payment amount must be greater than zero.");

        return new Payment(orderId, customerId, amount, method);
    }
}