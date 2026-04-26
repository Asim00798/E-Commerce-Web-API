#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.UserPreference.ValueObjects
{
    public sealed record UserId
    {
        public Guid Value { get; init; }

        public UserId(Guid value)
        {
            Value = value;
        }
    }
}

#endif