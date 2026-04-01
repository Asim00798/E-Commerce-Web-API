using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Specifications;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class CatalogSearchService
    {
        private readonly IProductRepository _productRepository;

        public CatalogSearchService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(
            string? query,
            CategoryId? categoryId,
            Money? minPrice,
            Money? maxPrice,
            bool includeOutOfStock,
            CancellationToken cancellationToken = default)
        {
            // Combine specifications dynamically
            var spec = new ProductSearchSpecification(query, categoryId, minPrice, maxPrice, includeOutOfStock);
            return await _productRepository.FindAsync(spec, cancellationToken);
        }
    }
}
