using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserRegistered : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserRegistered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}