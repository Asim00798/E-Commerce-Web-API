using E_Commerce.Domain.DomainEvents;
using System;

namespace E_Commerce.Application.ApplicationEvents.Reviews.Review
{
    public sealed class ReviewResponseAdded : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewResponseAdded(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}