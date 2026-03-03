using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.RefreshToken
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