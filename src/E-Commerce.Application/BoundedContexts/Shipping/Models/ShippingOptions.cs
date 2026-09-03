namespace E_Commerce.Application.BoundedContexts.Shipping.Models;

/// <summary>
/// Application-level shipping options.
/// Contains configuration values required by Shipping workflows.
/// </summary>
public sealed class ShippingOptions
{
    public const string SectionName = "Shipping";

    /// <summary>
    /// Maximum allowed delivery attempts before a shipment must be returned.
    /// This value is supplied to the domain from configuration.
    /// </summary>
    public int MaximumDeliveryAttempts { get; set; } = 3;
}