using System;

namespace E_Commerce.Domain.BoundedContexts.Reviews.Reviews.Review
{
    public sealed class ReviewDeleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewDeleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}