#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.ValueObjects
{
    public sealed record ModelParameters
    {
        public Dictionary<string, string> Config { get; init; }

        public ModelParameters(Dictionary<string, string> config)
        {
            Config = config;
        }

        public static ModelParameters Empty => new(new Dictionary<string, string>());
    }
}

#endif