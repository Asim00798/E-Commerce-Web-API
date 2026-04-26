#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.MarketPlace.AggregateRoots.MarketplaceListing.ValueObjects
{
    public sealed record ProductId
    {
        public Guid Value { get; init; }

        public ProductId(Guid value)
        {
            Value = value;
        }
    }
}

#endif