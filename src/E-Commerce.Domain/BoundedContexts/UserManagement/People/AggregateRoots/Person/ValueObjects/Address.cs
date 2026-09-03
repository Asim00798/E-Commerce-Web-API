using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Enums;
using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects
{
    public sealed record Address
    {
        public string Street { get; init; }
        public string City { get; init; }
        public AddressType Type { get; init; } = AddressType.Home;
        public string? LocationMapUrl { get; init; }

        public Address(string street, string city,
                       AddressType type = AddressType.Home, string? locationMapUrl = null)
        {
            Street = ValidateRequired("Street", street);
            City = ValidateRequired("City", city);
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
            return address;
        }
    }
}

