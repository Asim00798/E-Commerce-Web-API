using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Tax
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