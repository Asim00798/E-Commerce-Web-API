using System.Text.RegularExpressions;
using E_Commerce.Domain.Exceptions;

namespace E_Commerce.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }

        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new BusinessRuleViolationException("Email cannot be empty");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new BusinessRuleViolationException("Email format is invalid");

            Value = email;
        }

    }
}
