using System;

namespace E_Commerce.Domain.DomainEvents.Profiles.CustomerProfile
{
    public sealed class CustomerProfileDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public CustomerProfileDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}