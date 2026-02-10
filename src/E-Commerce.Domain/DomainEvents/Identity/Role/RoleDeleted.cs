using System;

namespace E_Commerce.Domain.DomainEvents.Identity.Role
{
    public sealed class RoleDeleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public RoleDeleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}