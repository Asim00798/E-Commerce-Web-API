using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Policies;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductRecommendationService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRecommendationPolicy _recommendationPolicy;

        public ProductRecommendationService(
            IProductRepository productRepository,
            IRecommendationPolicy recommendationPolicy)
        {
            _productRepository = productRepository;
            _recommendationPolicy = recommendationPolicy;
        }

        public async Task<IReadOnlyList<ProductId>> GetRecommendationsAsync(
            CustomerId customerId,
            int maxResults,
            CancellationToken cancellationToken = default)
        {
            // Could load customer purchase history, browsing behavior, etc.
            // Here we delegate to a policy that encapsulates the recommendation algorithm.
            var recommendedIds = await _recommendationPolicy.GetRecommendationsAsync(customerId, maxResults, cancellationToken);
            return recommendedIds.ToList().AsReadOnly();
        }
    }
}
