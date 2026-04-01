using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.RefreshToken
{
    public sealed class RefreshTokenRefreshed : DomainEvent
    {
        public Guid AggregateId { get; }

        public RefreshTokenRefreshed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}