#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Events
{
    public class ProductListedDomainEvent : DomainEvent
    {
        public Guid ListingId { get; }
        public Guid ProductId { get; }
        public Guid SellerId { get; }

        public ProductListedDomainEvent(Guid listingId, Guid productId, Guid sellerId)
        {
            ListingId = listingId;
            ProductId = productId;
            SellerId = sellerId;
        }
    }
}

#endif