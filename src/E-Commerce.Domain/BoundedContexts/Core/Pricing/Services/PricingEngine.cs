#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Services
{
    public class PricingEngine
    {
        public Money CalculateFinalPrice(Money basePrice, List<IPricingStrategy> strategies, PricingContext context)
        {
            var composite = new CompositePricingStrategy();
            foreach (var strategy in strategies)
            {
                composite.AddStrategy(strategy);
            }

            return composite.CalculatePrice(basePrice, context);
        }
    }
}

#endif