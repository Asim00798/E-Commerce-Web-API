using System;

namespace E_Commerce.Domain.DomainEvents.Security.LoginAttempt
{
    public sealed class BruteForceDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public BruteForceDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}