#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Strategies
{
    public class BulkPricingStrategy : IPricingStrategy
    {
        private readonly int _threshold;
        private readonly decimal _bulkPercentage;

        public BulkPricingStrategy(int threshold, decimal bulkPercentage)
        {
            _threshold = threshold;
            _bulkPercentage = bulkPercentage;
        }

        public Money CalculatePrice(Money basePrice, PricingContext context)
        {
            if (context.Quantity >= _threshold)
            {
                var discountAmount = basePrice.Amount * (_bulkPercentage / 100);
                return new Money(basePrice.Amount - discountAmount, basePrice.Currency);
            }
            return basePrice;
        }
    }
}

#endif