using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;

public sealed partial class Shipment
{
    public void AssignDriver(Guid driverId)
    {
        if (driverId == Guid.Empty)
            throw new ShipmentException("Driver ID is required.");

        if (Status != ShipmentStatus.Created && Status != ShipmentStatus.Assigned)
            throw new ShipmentException("Shipment cannot be assigned in its current state.");

        if (Status == ShipmentStatus.Assigned && AssignedDriverId == driverId)
            return; // Idempotent

        AssignedDriverId = driverId;
        AssignedAtUtc = DateTime.UtcNow;
        Status = ShipmentStatus.Assigned;
    }

    public void ReassignDriver(Guid newDriverId)
    {
        if (newDriverId == Guid.Empty)
            throw new ShipmentException("New driver ID is required.");

        if (Status is not (ShipmentStatus.Assigned or ShipmentStatus.ReadyForPickup or ShipmentStatus.PickedUp or ShipmentStatus.OutForDelivery))
            throw new ShipmentException("Shipment cannot be reassigned in its current state.");

        if (AssignedDriverId == newDriverId)
            return;

        AssignedDriverId = newDriverId;
        AssignedAtUtc = DateTime.UtcNow;
    }

    public void MarkReadyForPickup()
    {
        if (Status != ShipmentStatus.Assigned)
            throw new ShipmentException("Shipment can only be marked ready for pickup from Assigned state.");

        Status = ShipmentStatus.ReadyForPickup;
        ReadyForPickupAtUtc = DateTime.UtcNow;
    }

    public void MarkPickedUp()
    {
        if (Status != ShipmentStatus.ReadyForPickup)
            throw new ShipmentException("Shipment can only be picked up from ReadyForPickup state.");

        Status = ShipmentStatus.PickedUp;
        PickedUpAtUtc = DateTime.UtcNow;
    }

    public void StartDelivery()
    {
        if (Status != ShipmentStatus.PickedUp)
            throw new ShipmentException("Shipment can only start delivery from PickedUp state.");

        Status = ShipmentStatus.OutForDelivery;
        OutForDeliveryAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ShipmentShippedDomainEvent(
            Id,
            OrderId,
            OutForDeliveryAtUtc.Value));
    }

    public void BeginReturn()
    {
        if (Status != ShipmentStatus.OutForDelivery)
            throw new ShipmentException("Shipment can only begin return from OutForDelivery state.");

        Status = ShipmentStatus.ReturnToSender;
    }

    public void CompleteReturn()
    {
        if (Status != ShipmentStatus.ReturnToSender)
            throw new ShipmentException("Shipment can only complete return from ReturnToSender state.");

        Status = ShipmentStatus.Returned;
        ReturnedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ShipmentReturnedDomainEvent(
            Id,
            OrderId,
            ReturnedAtUtc.Value));
    }

    public void Cancel()
    {
        if (Status is not (ShipmentStatus.Created or ShipmentStatus.Assigned or ShipmentStatus.ReadyForPickup))
            throw new ShipmentException("Shipment cannot be cancelled in its current state.");

        Status = ShipmentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ShipmentCancelledDomainEvent(
            Id,
            OrderId,
            CancelledAtUtc.Value));
    }
}