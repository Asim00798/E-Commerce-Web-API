using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
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