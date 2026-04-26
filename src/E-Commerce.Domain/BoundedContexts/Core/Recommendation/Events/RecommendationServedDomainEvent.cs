#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Events
{
    public class RecommendationServedDomainEvent : DomainEvent
    {
        public Guid RecommendationId { get; }
        public Guid? UserId { get; }

        public RecommendationServedDomainEvent(Guid recommendationId, Guid? userId)
        {
            RecommendationId = recommendationId;
            UserId = userId;
        }
    }
}

#endif