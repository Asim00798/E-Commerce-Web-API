#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.Coupon
{
    public sealed class CouponExpired : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif