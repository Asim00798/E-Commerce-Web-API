using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;

public interface IShipmentRepository : IRepository<Shipment>
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetActiveByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetCustomerShipmentsAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetDriverShipmentsAsync(Guid driverId, CancellationToken ct = default);
}