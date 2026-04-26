#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Rules
{
    public class DiscountPolicyRule : IBusinessRule
    {
        private readonly DiscountPolicy _policy;
        private readonly PricingContext _context;

        public DiscountPolicyRule(DiscountPolicy policy, PricingContext context)
        {
            _policy = policy;
            _context = context;
        }

        public bool IsSatisfied()
        {
            return _context.Quantity >= _policy.MinimumQuantity;
        }

        public string Message => $"Discount policy '{_policy.Name}' requires a minimum quantity of {_policy.MinimumQuantity}.";
    }
}

#endif