using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Discount
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