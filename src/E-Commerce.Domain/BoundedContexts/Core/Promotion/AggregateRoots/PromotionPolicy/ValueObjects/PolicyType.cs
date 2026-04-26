#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.AggregateRoots.PromotionPolicy.ValueObjects
{
    public sealed record PolicyType
    {
        public string Value { get; init; }

        public PolicyType(string value)
        {
            Value = value;
        }

        public static PolicyType Global => new("Global");
        public static PolicyType CustomerSpecific => new("CustomerSpecific");
        public static PolicyType CategorySpecific => new("CategorySpecific");
    }
}

#endif