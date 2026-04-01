using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax.Events.Tax
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