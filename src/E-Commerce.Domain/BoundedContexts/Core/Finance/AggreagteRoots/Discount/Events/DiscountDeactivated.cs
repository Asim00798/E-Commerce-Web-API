#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Discount.Discount
{
    public sealed class DiscountDeactivated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif