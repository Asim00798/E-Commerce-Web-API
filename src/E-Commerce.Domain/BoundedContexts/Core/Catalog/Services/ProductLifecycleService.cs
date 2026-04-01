using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Rules;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductLifecycleService
    {
        private readonly IProductRepository _productRepository;

        public ProductLifecycleService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task PublishProductAsync(ProductId productId, DateTime publishDate, string publishedBy, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // Use a rule to check if product can be published
            var rule = new ProductCanBePublishedRule(product);
            if (!rule.IsSatisfied())
                throw new BusinessRuleViolationException(rule.Message);

            product.Publish(publishDate, publishedBy);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        public async Task DiscontinueProductAsync(ProductId productId, string discontinuedBy, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            product.Discontinue(discontinuedBy);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }
    }
}
