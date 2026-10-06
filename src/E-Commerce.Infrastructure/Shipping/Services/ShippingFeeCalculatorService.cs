using E_Commerce.Application.BoundedContexts.Shipping.Abstractions;
using E_Commerce.Application.Shared.Shipping.Models;
using E_Commerce.Application.Shared.Shipping.Services;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Policies;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Infrastructure.Shipping.Services;

/// <summary>
/// Infrastructure implementation of the shared IShippingFeeCalculator.
/// Orchestrates distance resolution and domain fee policy application.
///
/// Domain business outcomes propagate as Shipping Domain exceptions.
/// Consumers (Ordering) catch them and surface the message as a business failure.
/// </summary>
public sealed class ShippingFeeCalculatorService : IShippingFeeCalculator
{
    private readonly ILocationService _locationService;
    private readonly LocalShippingFeePolicy _feePolicy;

    public ShippingFeeCalculatorService(
        ILocationService locationService,
        LocalShippingFeePolicy feePolicy)
    {
        _locationService = locationService;
        _feePolicy = feePolicy;
    }

    public async Task<ShippingFeeCalculationResult> CalculateAsync(
        ShippingFeeCalculationRequest request,
        CancellationToken ct = default)
    {
        Validate(request);

        var deliveryAddress = new DeliveryAddressSnapshot(
            request.FullName,
            request.PhoneNumber,
            request.Street,
            request.City,
            request.LocationMapUrl);

        ShippingDistance distance = await _locationService.GetDeliveryDistanceAsync(
            deliveryAddress,
            ct);

        ShippingFeeResult domainResult;
        try
        {
            domainResult = _feePolicy.CalculateFee(distance);
        }
        catch (DomainException ex)
        {
            // Re-wrap as ShipmentException so consumers see a Shipping-specific type
            // and can surface the message as a business failure.
            throw new ShipmentException(ex.Message);
        }

        return new ShippingFeeCalculationResult
        {
            Amount = domainResult.Amount,
            Currency = domainResult.Currency,
            DistanceKm = domainResult.Distance.Kilometers,
            CalculationBasis = domainResult.CalculationBasis
        };
    }

    private static void Validate(ShippingFeeCalculationRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Street))
            throw new ArgumentException("Street is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.City))
            throw new ArgumentException("City is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.LocationMapUrl))
            throw new ArgumentException("Location map URL is required.", nameof(request));
    }
}