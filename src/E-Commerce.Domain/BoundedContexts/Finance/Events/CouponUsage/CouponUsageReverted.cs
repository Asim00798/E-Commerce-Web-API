using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.CouponUsage
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