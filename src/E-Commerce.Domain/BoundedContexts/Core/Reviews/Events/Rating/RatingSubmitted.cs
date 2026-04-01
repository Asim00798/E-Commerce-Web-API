using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Rating
{
    public sealed class RatingSubmitted : DomainEvent
    {
        public Guid AggregateId { get; }

        public RatingSubmitted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}