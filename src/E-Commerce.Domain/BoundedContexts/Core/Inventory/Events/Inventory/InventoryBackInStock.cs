#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Inventory
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
#endif