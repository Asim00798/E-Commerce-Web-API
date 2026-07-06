using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Inventory.AggregateRoots
{
    public class Inventory : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Quantity Quantity { get; set; } = null!; // Total stock available
        public Quantity ReservedQuantity { get; set; } = null!; // Stock reserved for orders
        public int AvailableQuantity => Quantity.Value - ReservedQuantity.Value; // Computed property for available stock
        public int Count => Quantity.Value; // Total stock count
        public override void Validate()
        {
            base.Validate();

            if (Quantity.Value < 0)
                throw new BusinessRuleViolationException("Inventory Quantity", "Inventory Quantity cannot be negative.");

            if (ReservedQuantity.Value < 0)
                throw new BusinessRuleViolationException("Reserved Quantity","Reserved Quantity cannot be negative.");

            if (ReservedQuantity.Value > Quantity.Value)
                throw new BusinessRuleViolationException("Reserved Quantity","Reserved Quantity cannot exceed total Quantity.");
        }
    }
}