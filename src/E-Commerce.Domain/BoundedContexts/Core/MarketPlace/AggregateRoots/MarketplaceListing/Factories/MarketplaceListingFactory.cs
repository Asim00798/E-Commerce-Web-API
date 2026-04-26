#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using ListingAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Behaviors.MarketplaceListing;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Factories
{
    public static class MarketplaceListingFactory
    {
        public static ListingAggregate Create(SellerId sellerId, ProductId productId, PriceId priceId)
        {
            return new ListingAggregate(sellerId, productId, priceId);
        }
    }
}

#endif