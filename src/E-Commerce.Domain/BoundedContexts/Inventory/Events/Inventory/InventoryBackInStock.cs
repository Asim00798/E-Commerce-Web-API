using System;

namespace E_Commerce.Domain.BoundedContexts.Inventory.Inventory.Inventory
{
    public sealed class InventoryBackInStock : DomainEvent
    {
        public Guid InventoryBackInStockId { get; }

        public InventoryBackInStock(Guid inventoryBackInStockId)
        {
            InventoryBackInStockId = inventoryBackInStockId;
        }
    }
}