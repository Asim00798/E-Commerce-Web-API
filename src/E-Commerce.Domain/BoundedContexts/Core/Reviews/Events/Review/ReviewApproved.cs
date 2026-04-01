using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Review
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