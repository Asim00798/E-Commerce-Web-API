using System;

namespace E_Commerce.Domain.DomainEvents.Finance.Tax
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