using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Inventory.Inventory.Warehouse
{
    public sealed class WarehouseStockTransferredIn : DomainEvent
    {
        public Guid WarehouseStockTransferredInId { get; }

        public WarehouseStockTransferredIn(Guid warehouseStockTransferredInId)
        {
            WarehouseStockTransferredInId = warehouseStockTransferredInId;
        }
    }
}