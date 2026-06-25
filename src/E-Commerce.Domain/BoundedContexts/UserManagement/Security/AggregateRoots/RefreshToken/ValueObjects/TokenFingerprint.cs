using E_Commerce.Domain.SharedKernel.Exceptions;

namespace Domain.BoundedContexts.UserManagement.Security.AggregateRoots.RefreshToken.ValueObjects
{
    public sealed record TokenFingerprint
    {
        public string Hash { get; }

        public TokenFingerprint(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new BusinessRuleViolationException("Token fingerprint hash cannot be empty.");

            var normalized = hash.Trim();
            if (normalized.Length < 32)
                throw new BusinessRuleViolationException("Token fingerprint hash must be at least 32 characters.");

            Hash = normalized;
        }

        public override string ToString() => Hash;
    }
}
