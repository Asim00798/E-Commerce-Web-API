using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;

public sealed class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() 
    {
        // EF Core
    }

    public PaymentTransaction(
        PaymentTransactionType type,
        Money amount,
        string? providerTransactionId = null)
    {
        Type = type;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public PaymentTransactionType Type { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public string? ProviderTransactionId { get; private set; }
}