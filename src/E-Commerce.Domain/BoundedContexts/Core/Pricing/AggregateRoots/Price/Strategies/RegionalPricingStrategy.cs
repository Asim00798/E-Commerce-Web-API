#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public class RegionalPricingStrategy : IPricingStrategy
    {
        private readonly Region _region;
        private readonly decimal _adjustmentPercentage;

        public RegionalPricingStrategy(Region region, decimal adjustmentPercentage)
        {
            _region = region;
            _adjustmentPercentage = adjustmentPercentage;
        }

        public Money CalculatePrice(Money basePrice, PricingContext context)
        {
            // Placeholder: in a real scenario, context might have a RegionId
            // If the regions match, apply adjustment
            var adjustedAmount = basePrice.Amount * (1 + (_adjustmentPercentage / 100));
            return new Money(adjustedAmount, basePrice.Currency);
        }
    }
}

#endif