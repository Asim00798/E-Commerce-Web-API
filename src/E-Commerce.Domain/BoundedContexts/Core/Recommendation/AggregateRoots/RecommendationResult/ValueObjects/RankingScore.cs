#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects
{
    public sealed record RankingScore
    {
        public float Value { get; init; }

        public RankingScore(float value)
        {
            Value = value;
        }
    }
}

#endif