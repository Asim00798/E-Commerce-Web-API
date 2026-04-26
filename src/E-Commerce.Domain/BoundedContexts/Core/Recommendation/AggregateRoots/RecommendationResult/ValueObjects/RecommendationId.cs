#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects
{
    public sealed record RecommendationId
    {
        public Guid Value { get; init; }

        public RecommendationId(Guid value)
        {
            Value = value;
        }

        public static RecommendationId New() => new(Guid.NewGuid());
    }
}

#endif