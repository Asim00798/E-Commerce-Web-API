#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects
{
    public sealed record PreferenceVector
    {
        public Dictionary<string, float> Features { get; init; }

        public PreferenceVector(Dictionary<string, float> features)
        {
            Features = features;
        }

        public static PreferenceVector Empty => new(new Dictionary<string, float>());
    }
}

#endif