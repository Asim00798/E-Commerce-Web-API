using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Discount
{
    public sealed class DiscountRemoved : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}