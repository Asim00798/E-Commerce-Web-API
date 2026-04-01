using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon.Events.CouponUsage
{
    public sealed class CouponUsageReverted : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponUsageReverted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}