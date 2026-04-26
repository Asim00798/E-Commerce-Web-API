#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.CouponUsage
{
    public sealed class CouponUsageLimitReached : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponUsageLimitReached(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif