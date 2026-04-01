using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Domain.Catalog.Specifications;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Application.Catalog
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
            // Transaction management (e.g., IUnitOfWork) and logging can be added later as needed.
            var spec = new ProductSearchSpecification(query, categoryId, minPrice, maxPrice, includeOutOfStock);
            return await _productRepository.FindAsync(spec, cancellationToken);
        }
    }
}
