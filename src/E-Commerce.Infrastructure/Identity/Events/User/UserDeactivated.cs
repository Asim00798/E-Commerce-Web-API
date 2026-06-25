#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif