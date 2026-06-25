using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.ApiKey.ValueObjects
{
    public sealed record ApiKeyScope
    {
        public string Value { get; }

        public ApiKeyScope(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleViolationException("API key scope cannot be empty.");

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized.Length > 100)
                throw new BusinessRuleViolationException("API key scope cannot exceed 100 characters.");

            Value = normalized;
        }

        public bool Matches(ApiKeyScope requiredScope) =>
            Value == requiredScope.Value || Value == "*";

        public override string ToString() => Value;
    }
}
