#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.Entities
{
    public class ScoreComponent : BaseEntity
    {
        public string Name { get; private set; }
        public float RawValue { get; private set; }
        public float Weight { get; private set; }

        public ScoreComponent(string name, float rawValue, float weight)
        {
            Name = name;
            RawValue = rawValue;
            Weight = weight;
        }

        public float WeightedScore => RawValue * Weight;
    }
}

#endif