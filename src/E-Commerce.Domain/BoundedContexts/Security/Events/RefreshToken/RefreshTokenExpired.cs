using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.RefreshToken
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