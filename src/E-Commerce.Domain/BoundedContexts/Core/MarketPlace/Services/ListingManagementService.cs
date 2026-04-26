#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.Repositories;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Services
{
    public class ListingManagementService
    {
        private readonly IMarketplaceListingRepository _listingRepository;

        public ListingManagementService(IMarketplaceListingRepository listingRepository)
        {
            _listingRepository = listingRepository;
        }

        public async Task ActivateListingAsync(Guid listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing != null)
            {
                listing.Activate();
                await _listingRepository.UpdateAsync(listing);
            }
        }
    }
}

#endif