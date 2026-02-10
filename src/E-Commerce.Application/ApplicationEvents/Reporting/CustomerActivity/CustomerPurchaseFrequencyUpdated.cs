using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerPurchaseFrequencyUpdated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerPurchaseFrequencyUpdated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}