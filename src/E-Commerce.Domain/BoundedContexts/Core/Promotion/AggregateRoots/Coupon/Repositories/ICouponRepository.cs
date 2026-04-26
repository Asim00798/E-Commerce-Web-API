#if false
using CouponAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Behaviors.Coupon;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Repositories
{
    public interface ICouponRepository
    {
        Task<CouponAggregate?> GetByCodeAsync(string code);
        Task<IEnumerable<CouponAggregate>> GetAllAsync();
        Task AddAsync(CouponAggregate coupon);
        Task UpdateAsync(CouponAggregate coupon);
    }
}

#endif