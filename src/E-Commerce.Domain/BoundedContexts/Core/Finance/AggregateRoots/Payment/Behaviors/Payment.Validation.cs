using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Behaviors;

public sealed partial class Payment
{
    public override void Validate()
    {
        base.Validate();

        if (Amount.Amount <= 0)
            throw new PaymentException("Payment amount must be greater than zero.");
    }
}