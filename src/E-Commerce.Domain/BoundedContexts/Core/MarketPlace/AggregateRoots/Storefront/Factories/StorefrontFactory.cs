#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.ValueObjects;
using StorefrontAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Behaviors.Storefront;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Factories
{
    public static class StorefrontFactory
    {
        public static StorefrontAggregate Create(SellerId sellerId, string name)
        {
            var storeName = new StoreName(name);
            return new StorefrontAggregate(sellerId, storeName);
        }
    }
}

#endif