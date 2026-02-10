using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Coupon
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