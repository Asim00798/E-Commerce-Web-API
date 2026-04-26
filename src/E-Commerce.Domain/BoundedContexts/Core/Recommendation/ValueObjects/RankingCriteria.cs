#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.ValueObjects
{
    public sealed record RankingCriteria
    {
        public string Source { get; init; }
        public float RelevanceThreshold { get; init; }

        public RankingCriteria(string source, float relevanceThreshold = 0.5f)
        {
            Source = source;
            RelevanceThreshold = relevanceThreshold;
        }
    }
}

#endif