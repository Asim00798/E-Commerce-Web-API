#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Storefront.ValueObjects
{
    public sealed record StoreName
    {
        public string Value { get; init; }

        public StoreName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Store name cannot be empty");
            Value = value;
        }
    }
}

#endif