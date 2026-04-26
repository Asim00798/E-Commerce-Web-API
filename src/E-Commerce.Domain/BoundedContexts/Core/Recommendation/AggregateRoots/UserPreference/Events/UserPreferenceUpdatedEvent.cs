#if false
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Events
{
    public class UserPreferenceUpdatedEvent : DomainEvent
    {
        public Guid UserId { get; }

        public UserPreferenceUpdatedEvent(Guid userId)
        {
            UserId = userId;
        }
    }
}

#endif