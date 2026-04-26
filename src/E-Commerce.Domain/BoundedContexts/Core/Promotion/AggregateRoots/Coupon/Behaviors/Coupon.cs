#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.ValueObjects;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Behaviors
{
    public partial class Coupon : BaseEntity, IAggregateRoot
    {
        public CouponCode Code { get; private set; }
        public DiscountDescriptor Discount { get; private set; }
        public CouponLimit Limit { get; private set; }
        public CouponPeriod Period { get; private set; }
        public CouponStatus Status { get; private set; }

        private readonly List<CouponUsage> _usages = new();
        private readonly List<CouponRedemption> _redemptions = new();

        public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();
        public IReadOnlyCollection<CouponRedemption> Redemptions => _redemptions.AsReadOnly();

        public Coupon(CouponCode code, DiscountDescriptor discount, CouponLimit limit, CouponPeriod period)
        {
            Code = code;
            Discount = discount;
            Limit = limit;
            Period = period;
            Status = CouponStatus.Active;
        }

        public void Expire() => Status = CouponStatus.Expired;
    }
}

#endif