namespace E_Commerce.Infrastructure.Shipping.Location;

/// <summary>
/// Configuration for the company's local fulfillment location.
/// Used by the location service to calculate delivery distance.
/// </summary>
public sealed class LocationOptions
{
    public const string SectionName = "Shipping:Location";

    public double CompanyLatitude { get; set; }

    public double CompanyLongitude { get; set; }
}