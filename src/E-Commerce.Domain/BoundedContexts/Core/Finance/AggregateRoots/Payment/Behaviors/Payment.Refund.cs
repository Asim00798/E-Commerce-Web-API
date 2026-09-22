using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Enums;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;

public sealed partial class Payment
{
    public bool CanApplyRefund(Money refundAmount)
    {
        return IsPaymentInRefundableState() &&
               IsRefundAmountValid(refundAmount) &&
               IsSameCurrency(refundAmount) &&
               DoesNotExceedRemaining(refundAmount);
    }

    public void ApplyRefund(Money refundAmount)
    {
        if (!CanApplyRefund(refundAmount))
            throw new PaymentException("Refund is not eligible for the current payment state.");

        RefundedAmount = RefundedAmount.Add(refundAmount);

        Status = RefundedAmount.Amount == Amount.Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        _transactions.Add(new PaymentTransaction(
            PaymentTransactionType.Refund,
            refundAmount,
            ProviderTransactionId));
    }

    private bool IsPaymentInRefundableState()
    {
        return Status is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded;
    }

    private static bool IsRefundAmountValid(Money refundAmount)
    {
        return refundAmount.Amount > 0;
    }

    private bool IsSameCurrency(Money refundAmount)
    {
        return refundAmount.Currency == Amount.Currency;
    }

    private bool DoesNotExceedRemaining(Money refundAmount)
    {
        var remaining = Amount.Subtract(RefundedAmount);
        return refundAmount.Amount <= remaining.Amount;
    }
}