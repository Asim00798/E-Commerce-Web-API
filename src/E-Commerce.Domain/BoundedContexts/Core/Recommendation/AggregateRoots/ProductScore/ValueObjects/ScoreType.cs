#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects
{
    public sealed record ScoreType
    {
        public string Value { get; init; }

        public ScoreType(string value)
        {
            Value = value;
        }

        public static ScoreType Trending => new("Trending");
        public static ScoreType Popularity => new("Popularity");
        public static ScoreType Relevance => new("Relevance");
    }
}

#endif