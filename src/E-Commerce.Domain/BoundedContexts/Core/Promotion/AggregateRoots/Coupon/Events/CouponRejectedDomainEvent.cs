#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Events
{
    public class CouponRejectedDomainEvent : DomainEvent
    {
        public string Code { get; }
        public string Reason { get; }

        public CouponRejectedDomainEvent(string code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }
}

#endif