using System;

namespace E_Commerce.Domain.DomainEvents.Security.ApiKey
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