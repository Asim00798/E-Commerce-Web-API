using System;

namespace E_Commerce.Domain.BoundedContexts.Reviews.Reviews.Review
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