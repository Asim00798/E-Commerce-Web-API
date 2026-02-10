using System;

namespace E_Commerce.Domain.DomainEvents.Finance.CouponUsage
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