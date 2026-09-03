using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Behaviors;

public sealed partial class Shipment
{
    public override void Validate()
    {
        base.Validate();

        if (OrderId == Guid.Empty)
            throw new ShipmentException("Order ID is required.");

        if (CustomerId == Guid.Empty)
            throw new ShipmentException("Customer ID is required.");

        if (DeliveryAddress is null)
            throw new ShipmentException("Delivery address is required.");

        if (string.IsNullOrWhiteSpace(TrackingNumber))
            throw new ShipmentException("Tracking number is required.");
    }
}