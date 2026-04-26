#if false
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Rules
{
    public class CouponUsageLimitRule : IBusinessRule
    {
        private readonly int _currentUsages;
        private readonly int _maxUsages;

        public CouponUsageLimitRule(int currentUsages, int maxUsages)
        {
            _currentUsages = currentUsages;
            _maxUsages = maxUsages;
        }

        public bool IsSatisfied() => _currentUsages < _maxUsages;

        public string Message => "Coupon usage limit has been reached.";
    }
}

#endif