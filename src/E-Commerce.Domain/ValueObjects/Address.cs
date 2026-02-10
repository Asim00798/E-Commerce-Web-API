using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Address
    {
        public string Street { get; }
        public string City { get; }
        public string? State { get; }
        public string? PostalCode { get; }
        public string Country { get; }
        public AddressType Type { get; set; } = AddressType.Generic;
        public string? LocationMapUrl { get; set; } = null;
        public Address(string street, string city, string? state, string? postalCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new BusinessRuleViolationException("Street cannot be empty");

            if (string.IsNullOrWhiteSpace(city))
                throw new BusinessRuleViolationException("City cannot be empty");

            if (string.IsNullOrWhiteSpace(country))
                throw new BusinessRuleViolationException("Country cannot be empty");

            Street = street.Trim();
            City = city.Trim();
            State = state?.Trim();
            PostalCode = postalCode?.Trim();
            Country = country.Trim();
        }
    }
}
