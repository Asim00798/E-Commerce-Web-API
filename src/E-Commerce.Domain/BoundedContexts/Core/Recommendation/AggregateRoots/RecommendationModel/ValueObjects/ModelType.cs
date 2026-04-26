#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.ValueObjects
{
    public sealed record ModelType
    {
        public ModelTypeEnum Value { get; init; }

        public ModelType(ModelTypeEnum value)
        {
            Value = value;
        }

        public static ModelType Collaborative => new(ModelTypeEnum.CollaborativeFiltering);
        public static ModelType ContentBased => new(ModelTypeEnum.ContentBased);
    }
}

#endif