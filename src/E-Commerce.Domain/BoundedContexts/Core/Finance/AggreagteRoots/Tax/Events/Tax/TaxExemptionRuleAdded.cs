using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax.Events.Tax
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