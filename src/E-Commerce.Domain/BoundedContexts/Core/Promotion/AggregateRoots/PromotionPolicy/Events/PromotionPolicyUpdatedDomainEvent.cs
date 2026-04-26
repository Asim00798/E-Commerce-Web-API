#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.Events
{
    public class PromotionPolicyUpdatedDomainEvent : DomainEvent
    {
        public Guid PolicyId { get; }

        public PromotionPolicyUpdatedDomainEvent(Guid policyId)
        {
            PolicyId = policyId;
        }
    }
}

#endif