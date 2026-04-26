#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects
{
    public sealed record CouponLimit
    {
        public int MaxUsages { get; init; }
        public int MaxUsagesPerCustomer { get; init; }

        public CouponLimit(int maxUsages, int maxUsagesPerCustomer = 1)
        {
            MaxUsages = maxUsages;
            MaxUsagesPerCustomer = maxUsagesPerCustomer;
        }
    }
}

#endif