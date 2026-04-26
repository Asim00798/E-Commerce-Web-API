#if false
using E_Commerce.Domain.Exceptions;
using E_Commerce.Domain.SharedKernel.Enums;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.PersonalData.Enums
{
    public sealed record Address
    {
        public string Street { get; init; }
        public string City { get; init; }
        public string? State { get; init; }
        public string? PostalCode { get; init; }
        public string Country { get; init; }
        public AddressType Type { get; init; } = AddressType.Generic;
        public string? LocationMapUrl { get; init; }

        public Address(string street, string city, string? state, string? postalCode, string country,
                       AddressType type = AddressType.Generic, string? locationMapUrl = null)
        {
            Street = ValidateRequired("Street", street);
            City = ValidateRequired("City", city);
            Country = ValidateRequired("Country", country);

            State = state?.Trim();
            PostalCode = postalCode?.Trim();
            Type = type;
            LocationMapUrl = locationMapUrl;
        }

        // ======================
        // "With" methods with validation
        // ======================

        public Address WithStreet(string street) =>
            this with { Street = ValidateRequired("Street", street) };

        public Address WithCity(string city) =>
            this with { City = ValidateRequired("City", city) };

        public Address WithState(string? state)
        {
            if (state != null && state.Length > 100)
                throw new BusinessRuleViolationException("State cannot exceed 100 characters.");
            return this with { State = state?.Trim() };
        }

        public Address WithPostalCode(string? postalCode)
        {
            if (postalCode != null && postalCode.Length > 20)
                throw new BusinessRuleViolationException("Postal code cannot exceed 20 characters.");
            return this with { PostalCode = postalCode?.Trim() };
        }

        public Address WithCountry(string country) =>
            this with { Country = ValidateRequired("Country", country) };

        public Address WithType(AddressType type)
        {
            if (!Enum.IsDefined(typeof(AddressType), type))
                throw new BusinessRuleViolationException("Invalid address type.");
            return this with { Type = type };
        }

        public Address WithLocationMapUrl(string? url)
        {
            if (url != null && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new BusinessRuleViolationException("Invalid URL format for location map.");
            return this with { LocationMapUrl = url };
        }

        // ======================
        // Helpers
        // ======================

        private static string ValidateRequired(string propName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException($"{propName} cannot be empty.");
            return value.Trim();
        }

        public override string ToString()
        {
            var address = $"{Street}, {City}";
            if (!string.IsNullOrWhiteSpace(State)) address += $", {State}";
            if (!string.IsNullOrWhiteSpace(PostalCode)) address += $", {PostalCode}";
            address += $", {Country}";
            return address;
        }
    }
}

#endif