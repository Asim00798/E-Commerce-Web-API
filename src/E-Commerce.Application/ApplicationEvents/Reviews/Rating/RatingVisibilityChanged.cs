using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Rating
{
    public sealed class RatingVisibilityChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingVisibilityChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}