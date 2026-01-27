using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Inventory
{
    public class InventoryTransaction : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid? OrderItemId { get; set; } // Optional, if related to an order
        public int QuantityChanged { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;
        public string? Notes { get; set; }

        // Navigation
        public Product? Product { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (QuantityChanged == 0)
                throw new InvalidOperationException("QuantityChanged cannot be zero.");
        }
    }

    public enum InventoryTransactionType
    {
        StockIn = 1,
        StockOut = 2,
        Reserved = 3,
        Released = 4,
        Adjustment = 5
    }
}
