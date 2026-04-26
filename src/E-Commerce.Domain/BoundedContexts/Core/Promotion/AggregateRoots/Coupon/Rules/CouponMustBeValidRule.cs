#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Rules
{
    public class CouponMustBeValidRule : IBusinessRule
    {
        private readonly CouponPeriod _period;
        private readonly DateTime _currentDate;

        public CouponMustBeValidRule(CouponPeriod period, DateTime currentDate)
        {
            _period = period;
            _currentDate = currentDate;
        }

        public bool IsSatisfied() => _period.IsActive(_currentDate);

        public string Message => "Coupon is not within its valid period.";
    }
}

#endif