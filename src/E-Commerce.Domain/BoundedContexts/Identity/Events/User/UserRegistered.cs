using System;

namespace E_Commerce.Domain.Events.Identity.User
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