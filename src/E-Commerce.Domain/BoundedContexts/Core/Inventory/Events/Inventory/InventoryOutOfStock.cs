using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
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