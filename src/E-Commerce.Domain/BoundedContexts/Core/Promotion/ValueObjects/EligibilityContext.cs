#if false
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Promotion.ValueObjects
{
    public sealed record EligibilityContext
    {
        public Guid CustomerId { get; init; }
        public Guid? StorefrontId { get; init; }
        public decimal TotalAmount { get; init; }
        public List<Guid> ProductIds { get; init; } = new();

        public EligibilityContext(Guid customerId, decimal totalAmount)
        {
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}

#endif