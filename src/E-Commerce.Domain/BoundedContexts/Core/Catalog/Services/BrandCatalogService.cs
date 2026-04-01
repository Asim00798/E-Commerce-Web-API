using E_Commerce.Domain.Catalog.AggregateRoots.Brand;
using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.Catalog.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class BrandCatalogService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IProductRepository _productRepository;

        public BrandCatalogService(
            IBrandRepository brandRepository,
            IProductRepository productRepository)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
        }

        public async Task ApplyBrandRestrictionToAllProductsAsync(
            BrandId brandId,
            RestrictionType restrictionType,
            string restrictionValue,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken)
                ?? throw new BrandNotFoundException(brandId);

            // Add restriction to brand itself
            brand.AddRestriction(restrictionType, restrictionValue, updatedBy);

            // Find all products of this brand (using a specification)
            var products = await _productRepository.FindByBrandAsync(brandId, cancellationToken);

            foreach (var product in products)
            {
                product.AddRestriction(restrictionType, restrictionValue, updatedBy);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }

            await _brandRepository.UpdateAsync(brand, cancellationToken);
        }
    }
}
