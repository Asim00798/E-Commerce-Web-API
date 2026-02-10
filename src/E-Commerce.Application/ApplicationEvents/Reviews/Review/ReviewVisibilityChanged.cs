using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewVisibilityChanged : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewVisibilityChanged(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}