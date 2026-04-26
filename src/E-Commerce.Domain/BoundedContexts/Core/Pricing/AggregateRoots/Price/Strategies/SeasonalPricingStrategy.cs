#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public class SeasonalPricingStrategy : IPricingStrategy
    {
        private readonly DateTime _start;
        private readonly DateTime _end;
        private readonly decimal _percentage;

        public SeasonalPricingStrategy(DateTime start, DateTime end, decimal percentage)
        {
            _start = start;
            _end = end;
            _percentage = percentage;
        }

        public Money CalculatePrice(Money basePrice, PricingContext context)
        {
            if (context.DateTime >= _start && context.DateTime <= _end)
            {
                var discountAmount = basePrice.Amount * (_percentage / 100);
                return new Money(basePrice.Amount - discountAmount, basePrice.Currency);
            }
            return basePrice;
        }
    }
}

#endif