#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.ValueObjects
{
    public sealed record PolicyId
    {
        public Guid Value { get; init; }

        public PolicyId(Guid value)
        {
            Value = value;
        }

        public static PolicyId New() => new(Guid.NewGuid());
    }
}

#endif