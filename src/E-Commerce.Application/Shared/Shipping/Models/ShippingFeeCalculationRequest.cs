namespace E_Commerce.Application.Shared.Shipping.Models;

/// <summary>
/// Input required to calculate a shipping fee.
/// Kept as a plain application model so other contexts do not depend
/// on Shipping domain value objects.
/// </summary>
public sealed record ShippingFeeCalculationRequest
{
    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Street { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string LocationMapUrl { get; init; } = string.Empty;
}