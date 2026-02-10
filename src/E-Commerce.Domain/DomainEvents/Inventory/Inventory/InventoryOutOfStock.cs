using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
{
    public sealed class InventoryOutOfStock : DomainEvent
    {
        public Guid InventoryOutOfStockId { get; }

        public InventoryOutOfStock(Guid inventoryOutOfStockId)
        {
            InventoryOutOfStockId = inventoryOutOfStockId;
        }
    }
}