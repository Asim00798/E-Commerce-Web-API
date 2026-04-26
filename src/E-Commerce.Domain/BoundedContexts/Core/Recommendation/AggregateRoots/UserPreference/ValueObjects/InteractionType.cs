#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.Enums;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects
{
    public sealed record InteractionType
    {
        public InteractionTypeEnum Value { get; init; }

        public InteractionType(InteractionTypeEnum value)
        {
            Value = value;
        }

        public static InteractionType View => new(InteractionTypeEnum.View);
        public static InteractionType Purchase => new(InteractionTypeEnum.Purchase);
    }
}

#endif