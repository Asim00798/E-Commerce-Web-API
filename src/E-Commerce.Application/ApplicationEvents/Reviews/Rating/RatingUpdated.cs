using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Rating
{
    public sealed class RatingUpdated : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingUpdated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}