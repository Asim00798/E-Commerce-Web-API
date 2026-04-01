using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Policies;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductDuplicateDetectionService
    {
        private readonly IProductRepository _productRepository;
        private readonly IDuplicateDetectionPolicy _duplicatePolicy;

        public ProductDuplicateDetectionService(
            IProductRepository productRepository,
            IDuplicateDetectionPolicy duplicatePolicy)
        {
            _productRepository = productRepository;
            _duplicatePolicy = duplicatePolicy;
        }

        public async Task<IReadOnlyList<Product>> FindDuplicatesAsync(Product candidate, CancellationToken cancellationToken = default)
        {
            // Get potential duplicates based on some criteria (e.g., same brand, name similar)
            var potential = await _productRepository.FindPotentialDuplicatesAsync(candidate, cancellationToken);

            // Use policy to filter actual duplicates
            var duplicates = new List<Product>();
            foreach (var p in potential)
            {
                if (await _duplicatePolicy.IsDuplicateAsync(candidate, p, cancellationToken))
                    duplicates.Add(p);
            }
            return duplicates.AsReadOnly();
        }
    }
}
