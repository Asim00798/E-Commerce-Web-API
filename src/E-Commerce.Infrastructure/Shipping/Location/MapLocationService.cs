using System.Globalization;
using System.Text.RegularExpressions;
using E_Commerce.Application.BoundedContexts.Shipping.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Shipping.Location;

/// <summary>
/// Local distance calculation service.
/// Parses coordinates from a Google Maps URL and calculates straight-line distance
/// from the configured company location using the Haversine formula.
/// This is suitable for the current local delivery model.
/// </summary>
public sealed class MapLocationService : ILocationService
{
    private readonly LocationOptions _options;
    private readonly ILogger<MapLocationService> _logger;

    private static readonly Regex CoordinatesRegex = new(
        @"(?<lat>-?\d{1,3}(?:\.\d+)?)\s*,\s*(?<lng>-?\d{1,3}(?:\.\d+)?)",
        RegexOptions.Compiled);

    public MapLocationService(
        IOptions<LocationOptions> options,
        ILogger<MapLocationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<ShippingDistance> GetDeliveryDistanceAsync(
        DeliveryAddressSnapshot deliveryAddress,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var destination = ParseCoordinates(deliveryAddress.LocationMapUrl);

        var distanceKm = CalculateHaversineDistance(
            _options.CompanyLatitude,
            _options.CompanyLongitude,
            destination.Latitude,
            destination.Longitude);

        var shippingDistance = new ShippingDistance(
            decimal.Round((decimal)distanceKm, 2, MidpointRounding.AwayFromZero));

        _logger.LogDebug(
            "Calculated local delivery distance {DistanceKm} km for address {Street}, {City}",
            shippingDistance.Kilometers,
            deliveryAddress.Street,
            deliveryAddress.City);

        return Task.FromResult(shippingDistance);
    }

    private static (double Latitude, double Longitude) ParseCoordinates(
        string locationMapUrl)
    {
        if (string.IsNullOrWhiteSpace(locationMapUrl))
        {
            throw new InvalidOperationException("Location map URL is missing.");
        }

        var match = CoordinatesRegex.Match(locationMapUrl);

        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"Unable to parse coordinates from location map URL: {locationMapUrl}");
        }

        var latitude = double.Parse(
            match.Groups["lat"].Value,
            CultureInfo.InvariantCulture);

        var longitude = double.Parse(
            match.Groups["lng"].Value,
            CultureInfo.InvariantCulture);

        return (latitude, longitude);
    }

    private static double CalculateHaversineDistance(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude)
    {
        const double earthRadiusKm = 6371.0;

        var lat1 = ToRadians(originLatitude);
        var lng1 = ToRadians(originLongitude);
        var lat2 = ToRadians(destinationLatitude);
        var lng2 = ToRadians(destinationLongitude);

        var deltaLat = lat2 - lat1;
        var deltaLng = lng2 - lng1;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}