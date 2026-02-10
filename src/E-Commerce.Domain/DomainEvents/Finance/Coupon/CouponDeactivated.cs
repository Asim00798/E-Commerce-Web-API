using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Coupon
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