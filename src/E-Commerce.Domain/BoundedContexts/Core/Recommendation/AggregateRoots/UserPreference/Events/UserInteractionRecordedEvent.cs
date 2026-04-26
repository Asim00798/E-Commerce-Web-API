#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Events
{
    public class UserInteractionRecordedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public Guid ProductId { get; }
        public string InteractionType { get; }

        public UserInteractionRecordedEvent(Guid userId, Guid productId, string interactionType)
        {
            UserId = userId;
            ProductId = productId;
            InteractionType = interactionType;
        }
    }
}

#endif