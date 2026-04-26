#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects
{
    public sealed record PriceId
    {
        public Guid Value { get; init; }

        public PriceId(Guid value)
        {
            Value = value;
        }
    }
}

#endif