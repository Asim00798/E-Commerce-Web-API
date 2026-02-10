using System;

namespace E_Commerce.Domain.DomainEvents.Inventory.Warehouse
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