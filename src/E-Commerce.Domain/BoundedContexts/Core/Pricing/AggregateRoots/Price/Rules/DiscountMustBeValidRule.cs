#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.Enums;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.Rules
{
    public class DiscountMustBeValidRule : IBusinessRule
    {
        private readonly Discount _discount;

        public DiscountMustBeValidRule(Discount discount)
        {
            _discount = discount;
        }

        public bool IsSatisfied()
        {
            if (_discount.Type == DiscountType.Percentage)
            {
                return _discount.Value >= 0 && _discount.Value <= 100;
            }
            return _discount.Value >= 0;
        }

        public string Message => "Discount value is not valid for its type.";
    }
}

#endif