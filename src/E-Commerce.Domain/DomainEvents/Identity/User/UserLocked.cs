using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserLocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserLocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}