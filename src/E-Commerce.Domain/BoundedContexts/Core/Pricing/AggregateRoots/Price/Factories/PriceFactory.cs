#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using PriceAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Behaviors.Price;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Factories
{
    public static class PriceFactory
    {
        public static PriceAggregate Create(Guid productId, decimal baseAmount, string currencyCode = "USD")
        {
            var currency = new Currency(currencyCode);
            var money = new Money(baseAmount, currency);
            return new PriceAggregate(productId, money);
        }
    }
}

#endif