using System;

namespace E_Commerce.Domain.DomainEvents.Identity.Permission
{
    public sealed class PermissionDeleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public PermissionDeleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}