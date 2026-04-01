using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Rating
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