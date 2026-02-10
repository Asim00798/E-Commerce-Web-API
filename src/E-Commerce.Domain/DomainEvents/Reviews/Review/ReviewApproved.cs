using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Review
{
    public sealed class ReviewApproved : DomainEvent
    {
        public Guid AggregateId { get; }

        public ReviewApproved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}