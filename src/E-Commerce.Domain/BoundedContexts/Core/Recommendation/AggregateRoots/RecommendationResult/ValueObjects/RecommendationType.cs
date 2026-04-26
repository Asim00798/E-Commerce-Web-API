#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationResult.ValueObjects
{
    public sealed record RecommendationType
    {
        public RecommendationTypeEnum Value { get; init; }

        public RecommendationType(RecommendationTypeEnum value)
        {
            Value = value;
        }

        public static RecommendationType Personalized => new(RecommendationTypeEnum.Personalized);
        public static RecommendationType Trending => new(RecommendationTypeEnum.Trending);
    }
}

#endif