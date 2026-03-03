using System;

namespace E_Commerce.Domain.Events.Identity.User
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