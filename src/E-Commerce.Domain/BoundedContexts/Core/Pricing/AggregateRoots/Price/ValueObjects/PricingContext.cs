#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Pricing.AggregateRoots.Price.ValueObjects
{
    public sealed record PricingContext
    {
        public Guid ProductId { get; init; }
        public Guid? CustomerId { get; init; }
        public DateTime DateTime { get; init; } = DateTime.UtcNow;
        public int Quantity { get; init; } = 1;

        public PricingContext(Guid productId, Guid? customerId = null, int quantity = 1)
        {
            ProductId = productId;
            CustomerId = customerId;
            Quantity = quantity;
        }
    }
}

#endif