using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects
{
    public sealed record TokenMetadata
    {
        public DateTime IssuedAt { get; }
        public DateTime ExpiresAt { get; }
        public DateTime? LastUsedAt { get; }

        public TokenMetadata(DateTime issuedAt, DateTime expiresAt, DateTime? lastUsedAt = null)
        {
            if (issuedAt == default)
                throw new BusinessRuleViolationException("Token issue timestamp is required.");

            if (expiresAt <= issuedAt)
                throw new BusinessRuleViolationException("Token expiration must be after issue time.");

            if (lastUsedAt.HasValue && lastUsedAt.Value < issuedAt)
                throw new BusinessRuleViolationException("Last used timestamp cannot precede issue time.");

            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            LastUsedAt = lastUsedAt;
        }

        public bool IsExpired(DateTime at) => at >= ExpiresAt;

        public TokenMetadata WithLastUsedAt(DateTime lastUsedAt) =>
            new(IssuedAt, ExpiresAt, lastUsedAt);
    }
}
