#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserUnlocked : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserUnlocked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif