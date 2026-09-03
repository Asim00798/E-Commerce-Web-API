using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects
{
    public sealed record CredentialHash
    {
        public string Value { get; }

        public CredentialHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new BusinessRuleViolationException("Password hash cannot be empty.");
            Value = hash;
        }

        public override string ToString() => Value;
    }
}