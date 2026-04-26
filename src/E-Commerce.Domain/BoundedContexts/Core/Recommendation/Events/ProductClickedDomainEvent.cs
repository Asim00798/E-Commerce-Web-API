#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Events
{
    public class ProductClickedDomainEvent : DomainEvent
    {
        public Guid ProductId { get; }
        public Guid? UserId { get; }

        public ProductClickedDomainEvent(Guid productId, Guid? userId)
        {
            ProductId = productId;
            UserId = userId;
        }
    }
}

#endif