#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Entities
{
    public class CouponRedemption : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public DateTime RedeemedAt { get; private set; }

        public CouponRedemption(Guid orderId, decimal discountAmount)
        {
            OrderId = orderId;
            DiscountAmount = discountAmount;
            RedeemedAt = DateTime.UtcNow;
        }
    }
}

#endif