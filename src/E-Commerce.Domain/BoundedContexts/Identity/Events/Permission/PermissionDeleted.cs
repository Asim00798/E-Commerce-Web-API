using System;

namespace E_Commerce.Domain.Events.Identity.Permission
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