using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using InventoryAR = E_Commerce.Domain.BoundedContexts.Core.Inventory.AggregateRoots.Inventory;

namespace E_Commerce.Domain.BoundedContexts.Core.Inventory.Repositories
{
    public interface IInventoryRepository : IRepository<InventoryAR>
    {
        public Task<InventoryAR> CreateAsync(InventoryAR inventoryAR);
        public Task<InventoryAR> FindAndApplyAdjustmentsAsync();
    }
}
