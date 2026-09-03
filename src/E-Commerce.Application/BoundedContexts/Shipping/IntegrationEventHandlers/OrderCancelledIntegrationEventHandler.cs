using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Application.BoundedContexts.Shipping.IntegrationEventHandlers;

public sealed class OrderCancelledIntegrationEventHandler
    : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCancelledIntegrationEventHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        OrderCancelledIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        var shipment = await _shipmentRepository.GetActiveByOrderIdAsync(
            integrationEvent.OrderId,
            ct);

        if (shipment is null)
            return; // nothing to cancel

        try
        {
            shipment.Cancel();
            await _shipmentRepository.UpdateAsync(shipment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DomainException)
        {
            // Shipment already terminal or not cancellable.
            // Treat as successfully handled.
        }
    }
}