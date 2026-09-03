using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;

public sealed partial class Cart
{
    public override void Validate()
    {
        base.Validate();

        if (CustomerId == Guid.Empty)
            throw new CartException("Customer ID is required.");
    }
}