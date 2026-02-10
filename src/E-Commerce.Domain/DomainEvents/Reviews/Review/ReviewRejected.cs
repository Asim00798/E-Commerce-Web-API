using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Review
{
    public sealed class ReviewRejected : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewRejected(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}