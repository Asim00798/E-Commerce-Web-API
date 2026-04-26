#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Entities;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects;
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Behaviors
{
    public partial class UserPreference : BaseEntity, IAggregateRoot
    {
        public UserId UserId { get; private set; }
        public PreferenceVector Vector { get; private set; }

        private readonly List<UserInteraction> _interactions = new();
        private readonly List<PreferenceSignal> _signals = new();

        public IReadOnlyCollection<UserInteraction> Interactions => _interactions.AsReadOnly();
        public IReadOnlyCollection<PreferenceSignal> Signals => _signals.AsReadOnly();

        public UserPreference(UserId userId)
        {
            UserId = userId;
            Vector = PreferenceVector.Empty;
        }

        public void RecordInteraction(Guid productId, InteractionType type)
        {
            _interactions.Add(new UserInteraction(productId, type));
        }
    }
}

#endif