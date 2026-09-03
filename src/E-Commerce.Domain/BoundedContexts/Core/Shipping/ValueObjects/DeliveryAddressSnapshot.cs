using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.ValueObjects;

/// <summary>
/// Immutable snapshot of the local delivery address used for a shipment.
/// It contains only the information needed for local delivery.
/// This value object is owned by the Shipping bounded context.
/// </summary>
public sealed record DeliveryAddressSnapshot
{
    public string FullName { get; init; }
    public string PhoneNumber { get; init; }
    public string Street { get; init; }
    public string City { get; init; }
    public string LocationMapUrl { get; init; }

    public DeliveryAddressSnapshot(
        string fullName,
        string phoneNumber,
        string street,
        string city,
        string locationMapUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("Full name is required.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessRuleViolationException("Phone number is required.");

        if (string.IsNullOrWhiteSpace(street))
            throw new BusinessRuleViolationException("Street is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new BusinessRuleViolationException("City is required.");

        if (string.IsNullOrWhiteSpace(locationMapUrl))
            throw new BusinessRuleViolationException("Location map URL is required.");

        FullName = fullName;
        PhoneNumber = phoneNumber;
        Street = street;
        City = city;
        LocationMapUrl = locationMapUrl;
    }
}