#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.Behaviors
{
    public partial class RecommendationResult : BaseEntity, IAggregateRoot
    {
        public RecommendationId RecommendationId { get; private set; }
        public Guid? UserId { get; private set; }
        public RecommendationType Type { get; private set; }
        public DateTime GeneratedAt { get; private set; }

        private readonly List<RecommendedItem> _items = new();
        public IReadOnlyCollection<RecommendedItem> Items => _items.AsReadOnly();

        public RecommendationResult(RecommendationId recommendationId, Guid? userId, RecommendationType type)
        {
            RecommendationId = recommendationId;
            UserId = userId;
            Type = type;
            GeneratedAt = DateTime.UtcNow;
        }

        public void AddItem(Guid productId, float score)
        {
            _items.Add(new RecommendedItem(productId, new RankingScore(score)));
        }
    }
}

#endif