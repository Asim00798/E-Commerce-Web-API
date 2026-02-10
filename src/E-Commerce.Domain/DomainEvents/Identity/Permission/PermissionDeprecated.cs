using System;

namespace E_Commerce.Domain.DomainEvents.Identity.Permission
{
    public sealed class PermissionDeprecated : DomainEvent
    {
        public Guid AggregateId { get; }

        public PermissionDeprecated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}