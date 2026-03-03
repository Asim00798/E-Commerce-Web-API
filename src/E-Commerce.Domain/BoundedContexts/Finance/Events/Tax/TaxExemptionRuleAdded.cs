using System;

namespace E_Commerce.Domain.BoundedContexts.Finance.Finance.Tax
{
    public sealed class TaxExemptionRuleAdded : DomainEvent
    {
        public Guid AggregateId { get;}

        public TaxExemptionRuleAdded(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}