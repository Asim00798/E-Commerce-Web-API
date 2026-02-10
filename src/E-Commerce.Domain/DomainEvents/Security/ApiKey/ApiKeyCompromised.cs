using System;

namespace E_Commerce.Domain.DomainEvents.Security.ApiKey
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