using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.Catalog.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductAttributeService
    {
        private readonly IProductRepository _productRepository;

        public ProductAttributeService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task ValidateAttributesAsync(ProductId productId, IEnumerable<ProductAttribute> attributes, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            foreach (var attr in attributes)
            {
                if (!product.IsValidAttribute(attr))
                    throw new InvalidProductAttributeException($"Attribute {attr.Name} is not allowed for this product.");
            }
        }
    }
}
