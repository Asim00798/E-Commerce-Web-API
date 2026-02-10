using System;

namespace E_Commerce.Domain.DomainEvents.Security.RefreshToken
{
    public sealed class RefreshTokenRevoked : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenRevoked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}