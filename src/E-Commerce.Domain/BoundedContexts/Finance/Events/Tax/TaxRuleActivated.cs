using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Tax
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