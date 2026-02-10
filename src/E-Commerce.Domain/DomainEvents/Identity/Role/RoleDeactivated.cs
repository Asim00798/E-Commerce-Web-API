using System;

namespace E_Commerce.Domain.DomainEvents.Identity.Role
{
    public sealed class RoleDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public RoleDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}