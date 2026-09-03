using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;

public sealed partial class Shipment : BaseEntity, IAggregateRoot
{
    private readonly List<DeliveryAttempt> _deliveryAttempts = new();

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DeliveryAddressSnapshot DeliveryAddress { get; private set; } = null!;
    public ShipmentStatus Status { get; private set; }
    public Guid? AssignedDriverId { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;

    public DateTime? AssignedAtUtc { get; private set; }
    public DateTime? ReadyForPickupAtUtc { get; private set; }
    public DateTime? PickedUpAtUtc { get; private set; }
    public DateTime? OutForDeliveryAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? ReturnedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<DeliveryAttempt> DeliveryAttempts => _deliveryAttempts.AsReadOnly();

    private Shipment()
    {
        // EF Core
    }

    private Shipment(
        Guid orderId,
        Guid customerId,
        DeliveryAddressSnapshot deliveryAddress,
        string trackingNumber)
    {
        OrderId = orderId;
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        TrackingNumber = trackingNumber;
        Status = ShipmentStatus.Created;

        AddDomainEvent(new ShipmentCreatedDomainEvent(Id, OrderId));
    }

    public static Shipment Create(
        Guid orderId,
        Guid customerId,
        DeliveryAddressSnapshot deliveryAddress,
        string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ShipmentException("Tracking number is required.");

        return new Shipment(orderId, customerId, deliveryAddress, trackingNumber);
    }
}