#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects
{
    public sealed record ListingId
    {
        public Guid Value { get; init; }

        public ListingId(Guid value)
        {
            Value = value;
        }

        public static ListingId New() => new(Guid.NewGuid());
    }
}

#endif