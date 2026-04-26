#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.Coupon.Events
{
    public class CouponAppliedDomainEvent : DomainEvent
    {
        public string Code { get; }
        public Guid CustomerId { get; }

        public CouponAppliedDomainEvent(string code, Guid customerId)
        {
            Code = code;
            CustomerId = customerId;
        }
    }
}

#endif