#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects;
using CouponAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Behaviors.Coupon;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Factories
{
    public static class CouponFactory
    {
        public static CouponAggregate Create(string code, DiscountDescriptor discount, int maxUsages, DateTime end)
        {
            var couponCode = new CouponCode(code);
            var limit = new CouponLimit(maxUsages);
            var period = new CouponPeriod(DateTime.UtcNow, end);
            return new CouponAggregate(couponCode, discount, limit, period);
        }
    }
}

#endif