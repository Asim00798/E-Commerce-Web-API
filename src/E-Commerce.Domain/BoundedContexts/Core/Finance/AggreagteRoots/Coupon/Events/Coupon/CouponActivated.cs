using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.Coupon
{
    public sealed class CouponActivated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}