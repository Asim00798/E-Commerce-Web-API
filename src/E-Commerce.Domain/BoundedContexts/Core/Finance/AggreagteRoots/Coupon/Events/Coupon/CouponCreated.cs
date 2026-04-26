#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.Coupon
{
    public sealed class CouponCreated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif