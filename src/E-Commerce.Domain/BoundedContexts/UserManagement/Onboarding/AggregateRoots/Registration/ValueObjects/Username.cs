using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects
{
    public sealed record Username
    {
        public string Value { get; }

        public Username(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("Username cannot be empty.");
            Value = value.Trim().ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}