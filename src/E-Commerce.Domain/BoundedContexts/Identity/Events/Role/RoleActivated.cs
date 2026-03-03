using System;

namespace E_Commerce.Domain.Events.Identity.Role
{
    public sealed class RoleActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public RoleActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}