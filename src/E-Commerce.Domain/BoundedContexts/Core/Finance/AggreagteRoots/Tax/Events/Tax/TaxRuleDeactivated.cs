using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Tax.Events.Tax
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