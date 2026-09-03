namespace E_Commerce.Application.Shared.Shipping.Models;

/// <summary>
/// Shared shipping fee result returned to Ordering.
/// Contains only the information needed to finalize the order monetary snapshot.
/// </summary>
public sealed record ShippingFeeCalculationResult
{
    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public decimal DistanceKm { get; init; }

    public string CalculationBasis { get; init; } = string.Empty;
}