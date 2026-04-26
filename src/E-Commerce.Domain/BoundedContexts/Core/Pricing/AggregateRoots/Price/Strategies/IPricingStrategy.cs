#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public interface IPricingStrategy
    {
        Money CalculatePrice(Money basePrice, PricingContext context);
    }
}

#endif