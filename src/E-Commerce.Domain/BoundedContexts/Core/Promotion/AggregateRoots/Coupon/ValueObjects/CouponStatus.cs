#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects
{
    public sealed record CouponStatus
    {
        public CouponStatusEnum Value { get; init; }

        public CouponStatus(CouponStatusEnum value)
        {
            Value = value;
        }

        public static CouponStatus Active => new(CouponStatusEnum.Active);
        public static CouponStatus Expired => new(CouponStatusEnum.Expired);
    }
}

#endif