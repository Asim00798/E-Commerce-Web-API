using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;

public sealed partial class Shipment
{
    public void RecordDeliveryAttempt(
        DeliveryAttemptResult result,
        string? failureReason = null,
        string? notes = null)
    {
        if (Status != ShipmentStatus.OutForDelivery)
            throw new ShipmentException("Delivery attempt can only be recorded while out for delivery.");

        var attemptNumber = _deliveryAttempts.Count + 1;

        var attempt = new DeliveryAttempt(
            Id,
            attemptNumber,
            result,
            failureReason,
            notes);

        _deliveryAttempts.Add(attempt);

        if (result == DeliveryAttemptResult.Delivered)
        {
            Status = ShipmentStatus.Delivered;
            DeliveredAtUtc = DateTime.UtcNow;

            AddDomainEvent(new ShipmentDeliveredDomainEvent(
                Id,
                OrderId,
                DeliveredAtUtc.Value));
        }
    }

    public void Retry(int maximumDeliveryAttempts)
    {
        if (maximumDeliveryAttempts <= 0)
            throw new ShipmentException("Maximum delivery attempts must be positive.");

        if (Status != ShipmentStatus.OutForDelivery)
            throw new ShipmentException("Shipment cannot be retried in its current state.");

        var attemptCount = _deliveryAttempts.Count;

        if (attemptCount == 0)
            throw new ShipmentException("No delivery attempt exists to retry.");

        if (attemptCount >= maximumDeliveryAttempts)
        {
            BeginReturn();
            return;
        }

        // Retry is allowed. Shipment remains OutForDelivery.
        // The next RecordDeliveryAttempt represents the retry.
    }
}