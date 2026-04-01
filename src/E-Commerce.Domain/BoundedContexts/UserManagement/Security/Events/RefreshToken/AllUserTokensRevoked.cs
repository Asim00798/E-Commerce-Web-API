using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.RefreshToken
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