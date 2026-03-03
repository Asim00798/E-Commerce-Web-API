using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.RefreshToken
{
    public sealed class RefreshTokenCompromised : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenCompromised(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}