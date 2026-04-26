#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Events
{
    public class RecommendationGeneratedDomainEvent : DomainEvent
    {
        public Guid RecommendationId { get; }
        public Guid? UserId { get; }

        public RecommendationGeneratedDomainEvent(Guid recommendationId, Guid? userId)
        {
            RecommendationId = recommendationId;
            UserId = userId;
        }
    }
}

#endif