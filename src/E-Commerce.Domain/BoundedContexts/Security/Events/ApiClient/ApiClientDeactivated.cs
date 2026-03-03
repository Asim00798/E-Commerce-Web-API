using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.ApiClient
{
    public sealed class ApiClientDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiClientDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}