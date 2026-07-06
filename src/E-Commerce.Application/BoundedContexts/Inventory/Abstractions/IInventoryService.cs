
namespace E_Commerce.Application.BoundedContexts.Inventory.Abstractions
{
    public interface IInventoryService
    {
        Task ReleaseReservedStockAsync(Guid orderId, CancellationToken cancellationToken);
    }
}
