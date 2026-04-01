using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.LoginAttempt
{
    public sealed class SuspiciousLoginDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public SuspiciousLoginDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}