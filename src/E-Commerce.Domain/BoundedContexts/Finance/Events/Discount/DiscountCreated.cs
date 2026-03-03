using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Discount
{
    public sealed class DiscountCreated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}