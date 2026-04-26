#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Entities
{
    public class UserInteraction : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public InteractionType Type { get; private set; }
        public DateTime InteractionTime { get; private set; }

        public UserInteraction(Guid productId, InteractionType type)
        {
            ProductId = productId;
            Type = type;
            InteractionTime = DateTime.UtcNow;
        }
    }
}

#endif