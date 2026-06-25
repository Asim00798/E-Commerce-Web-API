#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif