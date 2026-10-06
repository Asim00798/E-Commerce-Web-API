using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.Policies;

public sealed class LocalShippingFeePolicy
{
    public ShippingFeeResult CalculateFee(ShippingDistance distance)
    {
        decimal amount;
        string basis;

        switch (distance.Kilometers)
        {
            case <= 5m:
                amount = 10m;
                basis = "Local 0-5 km";
                break;

            case <= 10m:
                amount = 15m;
                basis = "Local 5-10 km";
                break;

            case <= 20m:
                amount = 25m;
                basis = "Local 10-20 km";
                break;

            default:
                throw new ShipmentException(
                    $"Distance {distance.Kilometers} km is outside the service area.");
        }

        return new ShippingFeeResult(amount, "AED", distance, basis);
    }
}