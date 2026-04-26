#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public class CompositePricingStrategy : IPricingStrategy
    {
        private readonly List<IPricingStrategy> _strategies = new();

        public void AddStrategy(IPricingStrategy strategy) => _strategies.Add(strategy);

        public Money CalculatePrice(Money basePrice, PricingContext context)
        {
            var currentPrice = basePrice;
            foreach (var strategy in _strategies)
            {
                currentPrice = strategy.CalculatePrice(currentPrice, context);
            }
            return currentPrice;
        }
    }
}

#endif