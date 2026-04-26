#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public class PercentageDiscountStrategy : IPricingStrategy
    {
        private readonly decimal _percentage;

        public PercentageDiscountStrategy(decimal percentage)
        {
            _percentage = percentage;
        }

        public Money CalculatePrice(Money basePrice, PricingContext context)
        {
            var discountAmount = basePrice.Amount * (_percentage / 100);
            return new Money(basePrice.Amount - discountAmount, basePrice.Currency);
        }
    }
}

#endif