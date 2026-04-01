using E_Commerce.Domain.BoundedContexts.UserManagement.Identity;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Finance.AggreagteRoots.Coupon
{
    /// <summary>
    /// Immutable audit record representing a single usage of a coupon by a user.
    /// This entity has no business behavior and exists only for traceability.
    /// </summary>
    public class CouponUsage : BaseEntity
    {
        public Guid CouponId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTimeOffset UsedAt { get; private set; }

        // Navigation (EF)
        public Coupon? Coupon { get; private set; }
        public User? User { get; private set; }

        public CouponUsage(Guid couponId, Guid userId)
        {
            CouponId = couponId;
            UserId = userId;
            UsedAt = DateTimeOffset.UtcNow;
        }
    }
}
