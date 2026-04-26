#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Discount.Discount
{
    public sealed class DiscountRemoved : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif