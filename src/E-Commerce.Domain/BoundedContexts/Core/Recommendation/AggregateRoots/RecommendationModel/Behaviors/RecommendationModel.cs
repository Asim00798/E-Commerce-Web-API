#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Behaviors
{
    public partial class RecommendationModel : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public ModelType Type { get; private set; }
        public ModelParameters DefaultParameters { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<ModelVersion> _versions = new();
        public IReadOnlyCollection<ModelVersion> Versions => _versions.AsReadOnly();

        public RecommendationModel(string name, ModelType type)
        {
            Name = name;
            Type = type;
            DefaultParameters = ModelParameters.Empty;
            IsActive = true;
        }

        public void DeployVersion(string versionTag)
        {
            _versions.Add(new ModelVersion(versionTag));
        }
    }
}

#endif