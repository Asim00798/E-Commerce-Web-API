using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using ShipmentAggregate = E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors.Shipment;

namespace E_Commerce.Infrastructure.Persistence.Modules.Shipping.Repositories;

public sealed class ShipmentRepository : Repository<ShipmentAggregate>,IShipmentRepository
{
    public ShipmentRepository(AppDbContext dbContext) : base(dbContext)
    {}

    /// <summary>
    /// Defines shipment statuses considered active.
    /// Used by <see cref="GetActiveByOrderIdAsync"/> to find an active shipment for an order.
    /// </summary>
    private static readonly ShipmentStatus[] ActiveStatuses =
    {
        ShipmentStatus.Created,
        ShipmentStatus.Assigned,
        ShipmentStatus.ReadyForPickup,
        ShipmentStatus.PickedUp,
        ShipmentStatus.OutForDelivery,
        ShipmentStatus.ReturnToSender
    };

    public async Task<ShipmentAggregate?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .Include(x => x.DeliveryAttempts)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task<ShipmentAggregate?> GetActiveByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .Include(x => x.DeliveryAttempts)
            .FirstOrDefaultAsync(x =>
                x.OrderId == orderId &&
                ActiveStatuses.Contains(x.Status),
                ct);
    }

    public async Task<IReadOnlyList<ShipmentAggregate>> GetCustomerShipmentsAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .AsNoTracking()
            .Include(x => x.DeliveryAttempts)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShipmentAggregate>> GetDriverShipmentsAsync(
        Guid driverId,
        CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .AsNoTracking()
            .Include(x => x.DeliveryAttempts)
            .Where(x => x.AssignedDriverId == driverId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}