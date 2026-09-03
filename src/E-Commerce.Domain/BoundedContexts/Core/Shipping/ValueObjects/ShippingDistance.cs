using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

/// <summary>
/// Represents a delivery distance in kilometers.
/// Used as input for shipping fee calculation.
/// </summary>
public sealed record ShippingDistance
{
    /// <summary>
    /// To represent fractional distances (e.g., 3.25 km) with precise decimal arithmetic
    /// instead of binary floating-point double, which can introduce rounding errors.
    /// </summary>
    public decimal Kilometers { get; }

    public ShippingDistance(decimal kilometers)
    {
        if (kilometers < 0)
            throw new BusinessRuleViolationException("Distance cannot be negative.");

        Kilometers = kilometers;
    }
}