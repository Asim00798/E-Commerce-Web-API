using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Inventory
{
    public class Inventory : BaseEntity
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 0;
        public int ReservedQuantity { get; set; } = 0; // Stock reserved for orders
        public int AvailableQuantity => Quantity - ReservedQuantity;

        // Navigation
        public Product? Product { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Quantity < 0)
                throw new InvalidOperationException("Inventory Quantity cannot be negative.");

            if (ReservedQuantity < 0)
                throw new InvalidOperationException("ReservedQuantity cannot be negative.");

            if (ReservedQuantity > Quantity)
                throw new InvalidOperationException("ReservedQuantity cannot exceed total Quantity.");
        }
    }
}
