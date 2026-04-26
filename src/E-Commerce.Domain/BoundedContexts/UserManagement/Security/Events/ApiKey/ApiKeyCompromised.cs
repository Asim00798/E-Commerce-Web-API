#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.ApiKey
{
    public sealed class ApiKeyCompromised : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiKeyCompromised(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif