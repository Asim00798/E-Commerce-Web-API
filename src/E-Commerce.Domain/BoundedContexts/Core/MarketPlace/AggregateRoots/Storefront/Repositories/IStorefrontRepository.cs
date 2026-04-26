#if false
using StorefrontAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Behaviors.Storefront;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.Repositories
{
    public interface IStorefrontRepository
    {
        Task<StorefrontAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<StorefrontAggregate>> GetBySellerIdAsync(Guid sellerId);
        Task AddAsync(StorefrontAggregate storefront);
        Task UpdateAsync(StorefrontAggregate storefront);
    }
}

#endif