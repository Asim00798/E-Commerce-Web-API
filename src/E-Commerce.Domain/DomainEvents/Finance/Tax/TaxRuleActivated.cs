using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Tax
{
    public sealed class TaxRuleActivated : DomainEvent
    {
        public Guid AggregateId { get;}

        public TaxRuleActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}