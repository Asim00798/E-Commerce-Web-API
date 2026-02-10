using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Tax
{
    public sealed class TaxExemptionRuleRemoved : DomainEvent
    {
        public Guid AggregateId { get;}

        public TaxExemptionRuleRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}