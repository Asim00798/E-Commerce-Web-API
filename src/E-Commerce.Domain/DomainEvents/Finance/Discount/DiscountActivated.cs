using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Discount
{
    public sealed class DiscountActivated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}