#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Events
{
    public class ProductScoreCalculatedDomainEvent : DomainEvent
    {
        public Guid ProductId { get; }
        public float FinalScore { get; }

        public ProductScoreCalculatedDomainEvent(Guid productId, float finalScore)
        {
            ProductId = productId;
            FinalScore = finalScore;
        }
    }
}

#endif