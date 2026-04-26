#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.CouponUsage
{
    public sealed class CouponApplied : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif