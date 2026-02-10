using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
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