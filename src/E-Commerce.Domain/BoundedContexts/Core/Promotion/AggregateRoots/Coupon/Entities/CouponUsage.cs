#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Entities
{
    public class CouponUsage : BaseEntity
    {
        public Guid CustomerId { get; private set; }
        public DateTime UsedAt { get; private set; }

        public CouponUsage(Guid customerId)
        {
            CustomerId = customerId;
            UsedAt = DateTime.UtcNow;
        }
    }
}

#endif