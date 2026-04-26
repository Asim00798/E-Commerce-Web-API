#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Factories;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.Repositories;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.Services
{
    public class SellerOnboardingService
    {
        private readonly ISellerRepository _sellerRepository;

        public SellerOnboardingService(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task OnboardSellerAsync(string name)
        {
            var seller = SellerFactory.Create(name);
            await _sellerRepository.AddAsync(seller);
            // Additional onboarding logic
        }
    }
}

#endif