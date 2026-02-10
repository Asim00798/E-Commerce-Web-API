using System;

namespace E_Commerce.Application.ApplicationEvents.Reporting.CustomerActivity
{
    public sealed class CustomerSegmentChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerSegmentChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}