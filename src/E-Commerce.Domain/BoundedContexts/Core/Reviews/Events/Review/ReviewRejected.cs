#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Review
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
#endif