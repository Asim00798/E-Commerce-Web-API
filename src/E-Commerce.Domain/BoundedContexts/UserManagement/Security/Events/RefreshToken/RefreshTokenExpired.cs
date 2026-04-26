#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.RefreshToken
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
#endif