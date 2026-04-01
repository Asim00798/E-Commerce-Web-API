using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.CouponUsage
{
    public sealed class CouponRejected : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}