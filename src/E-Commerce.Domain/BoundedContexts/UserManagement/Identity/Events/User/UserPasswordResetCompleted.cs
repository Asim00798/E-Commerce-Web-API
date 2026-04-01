using System;

namespace E_Commerce.Domain.Events.Identity.User
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