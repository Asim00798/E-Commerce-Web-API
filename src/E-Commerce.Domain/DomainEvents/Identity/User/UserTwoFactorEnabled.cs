using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserTwoFactorEnabled : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserTwoFactorEnabled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}