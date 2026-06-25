#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserDeleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserDeleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif