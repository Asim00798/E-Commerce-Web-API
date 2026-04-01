using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.AggregateRoots.Brand;
using E_Commerce.Domain.Catalog.Policies;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductPricingService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IEnumerable<IDiscountPolicy> _discountPolicies;
        private readonly ITaxPolicy _taxPolicy;

        public ProductPricingService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IEnumerable<IDiscountPolicy> discountPolicies,
            ITaxPolicy taxPolicy)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _discountPolicies = discountPolicies;
            _taxPolicy = taxPolicy;
        }

        public async Task<Money> CalculateFinalPriceAsync(
            ProductId productId,
            Customer customer,
            DateTime purchaseDate,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            var brand = await _brandRepository.GetByIdAsync(product.BrandId, cancellationToken)
                ?? throw new BrandNotFoundException(product.BrandId);

            // Start with base price
            var currentPrice = product.BasePrice;

            // Apply brand‑level discount policies
            foreach (var policy in _discountPolicies.Where(p => p.AppliesTo(brand)))
            {
                currentPrice = policy.Apply(currentPrice, customer);
            }

            // Apply product‑specific promotions (if any)
            currentPrice = product.ApplyPromotionalDiscount(currentPrice, purchaseDate);

            // Apply tax
            currentPrice = _taxPolicy.AddTax(currentPrice, customer.Address);

            // Final invariant: price must be positive
            if (currentPrice.Amount <= 0)
                throw new DomainException("Calculated price must be positive.");

            return currentPrice;
        }
    }
}
