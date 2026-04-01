using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.AggregateRoots.Brand;
using E_Commerce.Domain.Catalog.Specifications;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductAvailabilityService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IInventoryIntegration _inventoryIntegration; // domain interface, implemented in infrastructure

        public ProductAvailabilityService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IInventoryIntegration inventoryIntegration)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _inventoryIntegration = inventoryIntegration;
        }

        public async Task<AvailabilityResult> CheckAvailabilityAsync(
            ProductId productId,
            CountryCode country,
            Channel channel,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            // 1. Product lifecycle status
            if (product.Status != ProductStatus.Published)
                return AvailabilityResult.NotAvailable("Product is not published.");

            // 2. Brand‑level geographic restrictions
            var brand = await _brandRepository.GetByIdAsync(product.BrandId, cancellationToken)
                ?? throw new BrandNotFoundException(product.BrandId);

            if (brand.HasRestriction(RestrictionType.Geographic, country.Code))
                return AvailabilityResult.NotAvailable("Brand does not sell in this country.");

            // 3. Product‑level geographic restrictions
            if (product.HasRestriction(RestrictionType.Geographic, country.Code))
                return AvailabilityResult.NotAvailable("Product cannot be sold in this country.");

            // 4. Channel restrictions
            if (brand.HasRestriction(RestrictionType.Channel, channel.ToString()) ||
                product.HasRestriction(RestrictionType.Channel, channel.ToString()))
                return AvailabilityResult.NotAvailable("Product not available on this channel.");

            // 5. Stock availability (from Inventory context)
            var stockLevel = await _inventoryIntegration.GetCurrentStockAsync(productId, cancellationToken);
            if (stockLevel <= 0)
                return AvailabilityResult.NotAvailable("Out of stock.");

            // 6. Pre‑order / backorder logic
            if (product.IsPreOrder && product.PreOrderAvailabilityDate > DateTime.UtcNow)
                return AvailabilityResult.PreOrder(product.PreOrderAvailabilityDate);

            return AvailabilityResult.Available();
        }
    }

    public record AvailabilityResult(
        bool IsAvailable,
        string? Reason = null,
        DateTime? ExpectedAvailabilityDate = null)
    {
        public static AvailabilityResult Available() => new(true);
        public static AvailabilityResult NotAvailable(string reason) => new(false, reason);
        public static AvailabilityResult PreOrder(DateTime expectedDate) => new(false, "Pre-order only", expectedDate);
    }
}
