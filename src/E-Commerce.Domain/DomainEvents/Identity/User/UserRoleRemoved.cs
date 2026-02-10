using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserRoleRemoved : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserRoleRemoved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}