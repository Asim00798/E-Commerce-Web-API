using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.LoginAttempt
{
    public sealed class AccountLockoutTriggered : DomainEvent
    {
        public Guid AggregateId { get; }

        public AccountLockoutTriggered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}