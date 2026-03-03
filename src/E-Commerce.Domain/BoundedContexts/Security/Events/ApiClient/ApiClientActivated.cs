using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.ApiClient
{
    public sealed class ApiClientActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiClientActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}