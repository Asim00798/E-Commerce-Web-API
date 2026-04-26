#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Security.Security.ApiClient
{
    public sealed class ApiClientSuspended : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiClientSuspended(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif