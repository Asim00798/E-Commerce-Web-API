using System;

namespace E_Commerce.Domain.BoundedContexts.Security.Security.ApiClient
{
    public sealed class ApiClientRegistered : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiClientRegistered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}