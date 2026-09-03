using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Behaviors;

public sealed partial class Order
{
    public override void Validate()
    {
        base.Validate();

        if (CustomerId == Guid.Empty)
            throw new OrderException("Customer ID is required.");

        if (_items.Count == 0)
            throw new OrderException("Order must have at least one item.");
    }
}