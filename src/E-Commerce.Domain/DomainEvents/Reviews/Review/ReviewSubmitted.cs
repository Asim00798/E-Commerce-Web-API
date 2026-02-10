using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Review
{
    public sealed class ReviewSubmitted : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewSubmitted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}