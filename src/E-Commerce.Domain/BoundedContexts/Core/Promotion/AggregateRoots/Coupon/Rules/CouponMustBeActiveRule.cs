#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;
using E_Commerce.Domain.SharedKernel.Rules;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Rules
{
    public class CouponMustBeActiveRule : IBusinessRule
    {
        private readonly CouponStatusEnum _status;

        public CouponMustBeActiveRule(CouponStatusEnum status)
        {
            _status = status;
        }

        public bool IsSatisfied() => _status == CouponStatusEnum.Active;

        public string Message => "Coupon is not active.";
    }
}

#endif