using System;

namespace E_Commerce.Domain.DomainEvents.Security.RefreshToken
{
    public sealed class RefreshTokenExpired : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenExpired(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}