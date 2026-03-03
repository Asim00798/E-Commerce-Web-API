using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.RefreshToken
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