using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserUnlocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserUnlocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}