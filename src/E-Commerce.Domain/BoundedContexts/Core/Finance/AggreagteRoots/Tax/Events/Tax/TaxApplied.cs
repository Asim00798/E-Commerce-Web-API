using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax.Events.Tax
{
    public sealed class TaxApplied : DomainEvent
    {
        public Guid AggregateId { get;}

        public TaxApplied(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}