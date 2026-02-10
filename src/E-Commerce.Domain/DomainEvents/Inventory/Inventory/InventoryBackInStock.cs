using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Inventory
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