#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.Seller.ValueObjects
{
    public sealed record SellerName
    {
        public string Value { get; init; }

        public SellerName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Seller name cannot be empty");
            Value = value;
        }
    }
}

#endif