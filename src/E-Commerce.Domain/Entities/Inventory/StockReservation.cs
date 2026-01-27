using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Inventory
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
