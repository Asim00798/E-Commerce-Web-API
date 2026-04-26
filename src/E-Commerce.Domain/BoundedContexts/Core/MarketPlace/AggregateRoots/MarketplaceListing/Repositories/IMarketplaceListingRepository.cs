#if false
using ListingAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Behaviors.MarketplaceListing;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Repositories
{
    public interface IMarketplaceListingRepository
    {
        Task<ListingAggregate?> GetByIdAsync(Guid id);
        Task<IEnumerable<ListingAggregate>> GetBySellerIdAsync(Guid sellerId);
        Task AddAsync(ListingAggregate listing);
        Task UpdateAsync(ListingAggregate listing);
    }
}

#endif