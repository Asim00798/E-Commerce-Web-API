using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Coupon
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