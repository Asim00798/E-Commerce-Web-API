#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseStockTransferredOut : DomainEvent
    {
        public Guid WarehouseStockTransferredOutId { get; }

        public WarehouseStockTransferredOut(Guid warehouseStockTransferredOutId)
        {
            WarehouseStockTransferredOutId = warehouseStockTransferredOutId;
        }
    }
}
#endif