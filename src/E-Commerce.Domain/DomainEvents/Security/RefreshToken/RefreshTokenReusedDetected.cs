using System;

namespace E_Commerce.Domain.DomainEvents.Security.RefreshToken
{
    public sealed class RefreshTokenReusedDetected : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenReusedDetected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}