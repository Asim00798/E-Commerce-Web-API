using System;

namespace E_Commerce.Domain.Events.Identity.Permission
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