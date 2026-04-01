using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Rating
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