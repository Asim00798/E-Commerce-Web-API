using System;

namespace E_Commerce.Domain.DomainEvents.Identity.User
{
    public sealed class UserPasswordResetCompleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public UserPasswordResetCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}