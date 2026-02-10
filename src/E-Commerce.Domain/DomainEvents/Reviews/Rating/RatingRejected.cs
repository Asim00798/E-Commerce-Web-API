using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Rating
{
    public sealed class RatingRejected : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}