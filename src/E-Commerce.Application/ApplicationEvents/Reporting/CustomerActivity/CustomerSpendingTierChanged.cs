using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerSpendingTierChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerSpendingTierChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}