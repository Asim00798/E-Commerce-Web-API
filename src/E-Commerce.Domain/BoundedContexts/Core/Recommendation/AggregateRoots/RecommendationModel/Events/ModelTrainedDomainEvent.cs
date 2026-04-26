#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.RecommendationModel.Events
{
    public class ModelTrainedDomainEvent : DomainEvent
    {
        public Guid ModelId { get; }
        public string Version { get; }

        public ModelTrainedDomainEvent(Guid modelId, string version)
        {
            ModelId = modelId;
            Version = version;
        }
    }
}

#endif