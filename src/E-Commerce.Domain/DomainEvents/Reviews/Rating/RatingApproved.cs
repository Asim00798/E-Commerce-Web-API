using System;

namespace E_Commerce.Domain.DomainEvents.Reviews.Rating
{
    public sealed class RatingApproved : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingApproved(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}