using E_Commerce.Domain.SharedKernel.Exceptions;
using System.Text.RegularExpressions;

namespace E_Commerce.Domain.SharedKernel.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; init; }

        public Email(string email)
        {
            Value = ValidateEmail(email);
        }

        // ======================
        // "With" method for immutability + validation
        // ======================
        public Email WithValue(string email) =>
            this with { Value = ValidateEmail(email) };

        // ======================
        // Validation helper
        // ======================
        private static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new BusinessRuleViolationException("Email cannot be empty");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new BusinessRuleViolationException("Email format is invalid");

            return email;
        }

        public override string ToString() => Value;
    }
}
