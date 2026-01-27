using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Identity;

namespace E_Commerce.Domain.Entities.Finance
{
    public class CouponUsage : BaseEntity
    {
        public Guid CouponId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public Coupon? Coupon { get; set; }
        public User? User { get; set; }
    }
}
