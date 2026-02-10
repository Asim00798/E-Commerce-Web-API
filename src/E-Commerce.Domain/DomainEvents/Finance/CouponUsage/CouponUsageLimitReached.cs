using System;

namespace E_Commerce.Domain.DomainEvents.Finance.CouponUsage
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