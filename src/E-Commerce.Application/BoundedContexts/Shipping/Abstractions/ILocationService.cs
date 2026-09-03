using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

namespace E_Commerce.Application.BoundedContexts.Shipping.Abstractions;

/// <summary>
/// Resolves the local delivery distance between the company's fulfillment location
/// and a customer delivery address.
/// Infrastructure implementations may use local coordinates or external map providers.
/// </summary>
public interface ILocationService
{
    Task<ShippingDistance> GetDeliveryDistanceAsync(
        DeliveryAddressSnapshot deliveryAddress,
        CancellationToken ct = default);
}