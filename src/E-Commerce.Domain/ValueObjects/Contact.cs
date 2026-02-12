using E_Commerce.Domain.Enums;
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Contact
    {
        public ContactType Type { get; init; }
        public string Value { get; init; }
        public bool IsPrimary { get; init; } = false;

        public Contact(ContactType type, string value, bool isPrimary = false)
        {
            Type = ValidateType(type);
            Value = ValidateValue(type, value);
            IsPrimary = isPrimary;
        }

        // ======================
        // "With" methods for immutability + validation
        // ======================

        public Contact WithType(ContactType type) =>
            this with { Type = ValidateType(type) };

        public Contact WithValue(string value) =>
            this with { Value = ValidateValue(this.Type, value) };

        public Contact WithIsPrimary(bool isPrimary) =>
            this with { IsPrimary = isPrimary };

        // ======================
        // Validation helpers
        // ======================

        private static ContactType ValidateType(ContactType type)
        {
            if (!Enum.IsDefined(typeof(ContactType), type))
                throw new BusinessRuleViolationException("Invalid contact type.");
            return type;
        }

        private static string ValidateValue(ContactType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Contact value cannot be empty.");
            if (value.Length > 200)
                throw new BusinessRuleViolationException("Contact value cannot exceed 200 characters.");

            // Optional stricter checks
            if (type == ContactType.Email && !value.Contains("@"))
                throw new BusinessRuleViolationException("Email contact must contain '@' symbol.");

            return value.Trim();
        }

        public override string ToString() =>
            $"{Type}: {Value}" + (IsPrimary ? " (Primary)" : "");
    }
}
