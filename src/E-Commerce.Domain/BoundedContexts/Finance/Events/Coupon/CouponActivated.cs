using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Coupon
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