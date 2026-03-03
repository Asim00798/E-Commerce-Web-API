using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Discount
{
    public sealed class DiscountExpired : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}