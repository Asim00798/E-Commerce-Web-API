#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects
{
    public sealed record ListingStatus
    {
        public string Value { get; init; }

        public ListingStatus(string value)
        {
            Value = value;
        }

        public static ListingStatus Draft => new("Draft");
        public static ListingStatus Active => new("Active");
        public static ListingStatus Deactivated => new("Deactivated");
    }
}

#endif