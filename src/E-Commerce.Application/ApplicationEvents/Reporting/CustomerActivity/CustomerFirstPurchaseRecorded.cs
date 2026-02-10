using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerFirstPurchaseRecorded : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerFirstPurchaseRecorded(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}