using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Discount
{
    public sealed class DiscountDeactivated : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}