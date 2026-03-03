using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Tax
{
    public sealed class TaxRuleDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public TaxRuleDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}