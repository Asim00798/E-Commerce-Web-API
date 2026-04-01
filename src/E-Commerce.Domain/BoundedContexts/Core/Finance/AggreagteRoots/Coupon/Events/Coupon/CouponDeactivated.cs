using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.Coupon
{
    public sealed class CouponDeactivated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}