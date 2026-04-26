#if false
using E_Commerce.Domain.SharedKernel.Entities;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.Entities
{
    public class PreferenceSignal : BaseEntity
    {
        public string SignalSource { get; private set; }
        public float Strength { get; private set; }

        public PreferenceSignal(string signalSource, float strength)
        {
            SignalSource = signalSource;
            Strength = strength;
        }
    }
}

#endif