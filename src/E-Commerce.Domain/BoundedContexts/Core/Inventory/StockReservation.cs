#if false
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoot.Product;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory
{
    public class StockReservation : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset ReservedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReleasedAt { get; set; }

        // Navigation
        public Product? Product { get; set; }
        public Order? Order { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Quantity <= 0)
                throw new InvalidOperationException("Reserved quantity must be greater than zero.");
        }
    }
}

#endif