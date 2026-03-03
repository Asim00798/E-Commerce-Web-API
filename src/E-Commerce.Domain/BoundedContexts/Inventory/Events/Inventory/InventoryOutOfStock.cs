using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
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