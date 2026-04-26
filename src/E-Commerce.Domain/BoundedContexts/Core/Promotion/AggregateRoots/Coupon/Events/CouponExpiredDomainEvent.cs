#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Events
{
    public class CouponExpiredDomainEvent : DomainEvent
    {
        public string Code { get; }

        public CouponExpiredDomainEvent(string code)
        {
            Code = code;
        }
    }
}

#endif