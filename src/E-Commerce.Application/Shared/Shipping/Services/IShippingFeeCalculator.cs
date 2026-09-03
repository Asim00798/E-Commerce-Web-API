using E_Commerce.Application.Shared.Shipping.Models;

namespace E_Commerce.Application.Shared.Shipping.Services;

/// <summary>
/// Shared cross-context contract for calculating shipping fees.
/// Used by Ordering during checkout/order pricing.
/// The Shipping bounded context implements this abstraction.
/// </summary>
public interface IShippingFeeCalculator
{
    Task<ShippingFeeCalculationResult> CalculateAsync(
        ShippingFeeCalculationRequest request,
        CancellationToken ct = default);
}