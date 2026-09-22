using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Events;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;

public sealed partial class Payment
{
    public void AssignProviderIntention(string provider, string intentionId)
    {
        EnsureCanAssignProviderIntention();
        EnsureProviderIsValid(provider);
        EnsureIntentionIdIsValid(intentionId);

        Provider = provider;
        ProviderIntentionId = intentionId;
        Status = PaymentStatus.AwaitingPayment;
    }

    public void Capture(string providerTransactionId)
    {
        EnsureCanCapture();
        EnsureProviderTransactionIdIsValid(providerTransactionId);

        ProviderTransactionId = providerTransactionId;
        Status = PaymentStatus.Captured;
        CompletedAtUtc = DateTime.UtcNow;

        RecordCaptureTransaction(providerTransactionId);
        AddPaymentCapturedDomainEvent(providerTransactionId);
    }

    public void Fail()
    {
        EnsureCanFail();

        Status = PaymentStatus.Failed;

        AddPaymentFailedDomainEvent();
    }

    public void Cancel()
    {
        EnsureCanCancel();

        Status = PaymentStatus.Cancelled;

        AddPaymentCancelledDomainEvent();
    }

    private void EnsureCanAssignProviderIntention()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new PaymentException(
                "Payment is not in a valid state to assign provider intention.");
        }
    }

    private static void EnsureProviderIsValid(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new PaymentException("Provider is required.");
        }
    }

    private static void EnsureIntentionIdIsValid(string intentionId)
    {
        if (string.IsNullOrWhiteSpace(intentionId))
        {
            throw new PaymentException("Provider intention ID is required.");
        }
    }

    private void EnsureCanCapture()
    {
        if (Status != PaymentStatus.AwaitingPayment)
        {
            throw new PaymentException(
                "Payment can only be captured when awaiting payment.");
        }
    }

    private static void EnsureProviderTransactionIdIsValid(string providerTransactionId)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
        {
            throw new PaymentException("Provider transaction ID is required.");
        }
    }

    private void EnsureCanFail()
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingPayment))
        {
            throw new PaymentException(
                "Payment cannot be failed in its current state.");
        }
    }

    private void EnsureCanCancel()
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.AwaitingPayment))
        {
            throw new PaymentException(
                "Payment cannot be cancelled in its current state.");
        }
    }

    private void RecordCaptureTransaction(string providerTransactionId)
    {
        _transactions.Add(new PaymentTransaction(
            PaymentTransactionType.Capture,
            Amount,
            providerTransactionId));
    }

    private void AddPaymentCapturedDomainEvent(string providerTransactionId)
    {
        AddDomainEvent(new PaymentCapturedDomainEvent(
            Id,
            OrderId,
            Amount,
            providerTransactionId));
    }

    private void AddPaymentFailedDomainEvent()
    {
        AddDomainEvent(new PaymentFailedDomainEvent(Id, OrderId));
    }

    private void AddPaymentCancelledDomainEvent()
    {
        AddDomainEvent(new PaymentCancelledDomainEvent(Id, OrderId));
    }
}