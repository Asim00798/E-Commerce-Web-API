using System;

namespace E_Commerce.Domain.DomainEvents.Security.RefreshToken
{
    public sealed class AllUserTokensRevoked : DomainEvent
    {
        public Guid AggregateId { get; }

        public AllUserTokensRevoked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}