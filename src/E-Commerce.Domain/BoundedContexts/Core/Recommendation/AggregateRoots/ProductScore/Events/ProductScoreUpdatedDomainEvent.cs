#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Events
{
    public class ProductScoreUpdatedDomainEvent : DomainEvent
    {
        public Guid ProductId { get; }
        public float OldScore { get; }
        public float NewScore { get; }

        public ProductScoreUpdatedDomainEvent(Guid productId, float oldScore, float newScore)
        {
            ProductId = productId;
            OldScore = oldScore;
            NewScore = newScore;
        }
    }
}

#endif