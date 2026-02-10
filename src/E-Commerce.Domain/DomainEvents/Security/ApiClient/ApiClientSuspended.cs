using System;

namespace E_Commerce.Domain.DomainEvents.Security.ApiClient
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