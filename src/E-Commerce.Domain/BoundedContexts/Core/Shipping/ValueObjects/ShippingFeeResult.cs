namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

/// <summary>
/// Represents the calculated shipping fee. This result is transient.
/// The Ordering context persists the accepted monetary value as Order.ShippingFee.
/// </summary>
public sealed record ShippingFeeResult(
    decimal Amount,
    string Currency,
    ShippingDistance Distance,
    string CalculationBasis);