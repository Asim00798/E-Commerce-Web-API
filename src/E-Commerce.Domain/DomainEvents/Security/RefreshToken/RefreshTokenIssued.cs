using System;

namespace E_Commerce.Domain.DomainEvents.Security.RefreshToken
{
    public sealed class RefreshTokenIssued : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenIssued(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}