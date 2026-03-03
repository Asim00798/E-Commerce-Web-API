using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.CouponUsage
{
    public sealed class CouponApplied : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public CouponApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}