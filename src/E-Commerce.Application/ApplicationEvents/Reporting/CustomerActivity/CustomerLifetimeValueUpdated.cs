using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerLifetimeValueUpdated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerLifetimeValueUpdated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}