#if false
using System.Linq.Expressions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.Enums;
using E_Commerce.Domain.SharedKernel.Specifications;
using CouponAggregate = E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Behaviors.Coupon;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Specifications
{
    public class ValidCouponSpecification : ISpecification<CouponAggregate>
    {
        public Expression<Func<CouponAggregate, bool>> ToExpression()
        {
            return coupon => coupon.Status.Value == CouponStatusEnum.Active && coupon.Period.StartDate <= DateTime.UtcNow && coupon.Period.EndDate >= DateTime.UtcNow;
        }

        public bool IsSatisfiedBy(CouponAggregate entity)
        {
            return entity.Status.Value == CouponStatusEnum.Active && entity.Period.IsActive(DateTime.UtcNow);
        }
    }
}

#endif