#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Discount.Discount
{
    public sealed class DiscountCreated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif