#if false
using System;

namespace E_Commerce.Domain.Events.Identity.Permission
{
    public sealed class PermissionReactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public PermissionReactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif