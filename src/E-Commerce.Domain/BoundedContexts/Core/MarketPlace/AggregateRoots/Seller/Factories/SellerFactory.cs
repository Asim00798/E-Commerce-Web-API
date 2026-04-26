#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects;
using SellerAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Behaviors.Seller;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Factories
{
    public static class SellerFactory
    {
        public static SellerAggregate Create(string name)
        {
            var sellerName = new SellerName(name);
            return new SellerAggregate(sellerName);
        }
    }
}

#endif