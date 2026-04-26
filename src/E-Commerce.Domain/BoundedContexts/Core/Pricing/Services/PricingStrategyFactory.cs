#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Services
{
    public class PricingStrategyFactory
    {
        public IPricingStrategy CreateStrategy(PricingStrategyType type, decimal value)
        {
            return type switch
            {
                PricingStrategyType.PercentageDiscount => new PercentageDiscountStrategy(value),
                // Other strategies would be implemented here with additional parameters if needed
                _ => throw new ArgumentException("Unsupported strategy type", nameof(type))
            };
        }
    }
}

#endif