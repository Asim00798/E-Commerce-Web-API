#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Recommendation.AggregateRoots.ProductScore.ValueObjects
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