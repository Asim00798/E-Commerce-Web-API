using E_Commerce.Application.BoundedContexts.Shipping.Abstractions;
using E_Commerce.Application.Shared.Shipping.Models;
using E_Commerce.Application.Shared.Shipping.Services;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Policies;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

namespace E_Commerce.Infrastructure.Shipping.Services;

/// <summary>
/// Infrastructure implementation of the shared IShippingFeeCalculator.
/// Orchestrates distance resolution and domain fee policy application.
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

        var deliveryAddress = new DeliveryAddressSnapshot(
            request.FullName,
            request.PhoneNumber,
            request.Street,
            request.City,
            request.LocationMapUrl);

        ShippingDistance distance = await _locationService.GetDeliveryDistanceAsync(
            deliveryAddress,
            ct);

        ShippingFeeResult domainResult = _feePolicy.CalculateFee(distance);

        return new ShippingFeeCalculationResult
        {
            Amount = domainResult.Amount,
            Currency = domainResult.Currency,
            DistanceKm = domainResult.Distance.Kilometers,
            CalculationBasis = domainResult.CalculationBasis
        };
    }
}