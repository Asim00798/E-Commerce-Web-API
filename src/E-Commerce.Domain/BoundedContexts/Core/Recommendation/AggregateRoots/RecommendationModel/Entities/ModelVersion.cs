#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Entities
{
    public class ModelVersion : BaseEntity
    {
        public string VersionTag { get; private set; }
        public DateTime DeployedAt { get; private set; }

        public ModelVersion(string versionTag)
        {
            VersionTag = versionTag;
            DeployedAt = DateTime.UtcNow;
        }
    }
}

#endif