using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Repositories;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Application.BoundedContexts.Shipping.IntegrationEventHandlers;

public sealed class OrderReadyForShippingIntegrationEventHandler
    : IIntegrationEventHandler<OrderReadyForShippingIntegrationEvent>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderReadyForShippingIntegrationEventHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        OrderReadyForShippingIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        var existing = await _shipmentRepository.GetActiveByOrderIdAsync(
            integrationEvent.OrderId,
            ct);

        if (existing is not null)
            return; // idempotent

        var address = new DeliveryAddressSnapshot(
            integrationEvent.FullName,
            integrationEvent.PhoneNumber,
            integrationEvent.Street,
            integrationEvent.City,
            integrationEvent.LocationMapUrl);

        var trackingNumber = $"TRK-{integrationEvent.OrderId.ToString("N").ToUpperInvariant()}";

        var shipment = Shipment.Create(
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            address,
            trackingNumber);

        await _shipmentRepository.AddAsync(shipment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}