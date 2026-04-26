#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.ValueObjects
{
    public sealed record StoreStatus
    {
        public string Value { get; init; }

        public StoreStatus(string value)
        {
            Value = value;
        }

        public static StoreStatus Active => new("Active");
        public static StoreStatus Maintenance => new("Maintenance");
        public static StoreStatus Suspended => new("Suspended");
    }
}

#endif