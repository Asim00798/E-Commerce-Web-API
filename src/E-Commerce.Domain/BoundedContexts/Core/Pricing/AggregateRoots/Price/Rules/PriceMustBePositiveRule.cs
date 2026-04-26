#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Rules
{
    public class PriceMustBePositiveRule : IBusinessRule
    {
        private readonly Money _price;

        public PriceMustBePositiveRule(Money price)
        {
            _price = price;
        }

        public bool IsSatisfied() => _price.Amount >= 0;

        public string Message => "Price amount must be positive.";
    }
}

#endif