using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Review
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