using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.RefreshToken
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