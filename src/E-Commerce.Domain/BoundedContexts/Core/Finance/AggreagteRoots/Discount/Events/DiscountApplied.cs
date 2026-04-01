using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Discount.Discount
{
    public sealed class DiscountApplied : DomainEvent
    {
        public Guid AggregateId { get; init; }

        public DiscountApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}