#if false
using System;

namespace E_Commerce.Domain.Events.Identity.User
{
    public sealed class UserTwoFactorDisabled : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserTwoFactorDisabled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif