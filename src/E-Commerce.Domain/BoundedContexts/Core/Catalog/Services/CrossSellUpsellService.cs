using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Policies;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;

namespace E_Commerce.Domain.Catalog.Services
{
    public class CrossSellUpsellService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICrossSellPolicy _crossSellPolicy;

        public CrossSellUpsellService(
            IProductRepository productRepository,
            ICrossSellPolicy crossSellPolicy)
        {
            _productRepository = productRepository;
            _crossSellPolicy = crossSellPolicy;
        }

        public async Task<IReadOnlyList<ProductId>> GetSuggestionsAsync(
            ProductId productId,
            SuggestionType type,
            int maxResults,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            var suggestions = type switch
            {
                SuggestionType.CrossSell => await _crossSellPolicy.GetCrossSellAsync(product, maxResults, cancellationToken),
                SuggestionType.Upsell => await _crossSellPolicy.GetUpsellAsync(product, maxResults, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return suggestions.ToList().AsReadOnly();
        }
    }

    public enum SuggestionType { CrossSell, Upsell }
}
