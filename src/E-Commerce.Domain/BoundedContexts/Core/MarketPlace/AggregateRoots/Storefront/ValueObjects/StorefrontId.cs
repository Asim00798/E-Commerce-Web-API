#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.ValueObjects
{
    public sealed record StorefrontId
    {
        public Guid Value { get; init; }

        public StorefrontId(Guid value)
        {
            Value = value;
        }

        public static StorefrontId New() => new(Guid.NewGuid());
    }
}

#endif