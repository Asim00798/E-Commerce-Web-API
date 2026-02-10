using System;

namespace E_Commerce.Domain.DomainEvents.Security.ApiKey
{
    public sealed class ApiKeyActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public ApiKeyActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}