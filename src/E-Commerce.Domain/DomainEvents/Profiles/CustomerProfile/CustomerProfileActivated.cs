using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.CustomerProfile
{
    public sealed class CustomerProfileActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}