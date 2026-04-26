#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.ValueObjects
{
    public sealed record ModelId
    {
        public Guid Value { get; init; }

        public ModelId(Guid value)
        {
            Value = value;
        }

        public static ModelId New() => new(Guid.NewGuid());
    }
}

#endif