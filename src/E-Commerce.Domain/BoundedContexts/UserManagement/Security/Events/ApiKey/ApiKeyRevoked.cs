#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.ApiKey
{
    public sealed class ApiKeyRevoked : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiKeyRevoked(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif