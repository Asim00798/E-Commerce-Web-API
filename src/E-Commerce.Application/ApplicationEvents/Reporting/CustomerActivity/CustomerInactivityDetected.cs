using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerInactivityDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerInactivityDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}