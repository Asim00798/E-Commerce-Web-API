using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserEmailConfirmed : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserEmailConfirmed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}