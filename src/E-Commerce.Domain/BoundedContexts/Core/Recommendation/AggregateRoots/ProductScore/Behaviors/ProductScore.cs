#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Behaviors
{
    public partial class ProductScore : BaseEntity, IAggregateRoot
    {
        public ProductId ProductId { get; private set; }
        public ScoreValue TotalScore { get; private set; }
        public ScoreType Type { get; private set; }
        public TimeWindow Window { get; private set; }

        private readonly List<ScoreComponent> _components = new();
        private readonly List<ScoreHistoryEntry> _history = new();

        public IReadOnlyCollection<ScoreComponent> Components => _components.AsReadOnly();
        public IReadOnlyCollection<ScoreHistoryEntry> History => _history.AsReadOnly();

        public ProductScore(ProductId productId, ScoreType type, TimeWindow window)
        {
            ProductId = productId;
            Type = type;
            Window = window;
            TotalScore = ScoreValue.Zero;
        }

        public void UpdateScore(float newValue)
        {
            TotalScore = new ScoreValue(newValue);
            _history.Add(new ScoreHistoryEntry(newValue));
        }
    }
}

#endif