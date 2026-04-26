#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Reviews.Reviews.Wishlist
{
    public sealed class WishlistArchived : DomainEvent
    {
        public Guid AggregateId { get; }

        public WishlistArchived(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif