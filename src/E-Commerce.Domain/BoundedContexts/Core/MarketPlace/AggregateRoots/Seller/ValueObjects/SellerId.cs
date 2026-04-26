#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects
{
    public sealed record SellerId
    {
        public Guid Value { get; init; }

        public SellerId(Guid value)
        {
            Value = value;
        }

        public static SellerId New() => new(Guid.NewGuid());
    }
}

#endif