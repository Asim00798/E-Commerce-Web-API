namespace E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.AggregateRoots.Registration.ValueObjects
{
    /// <summary>
    /// Immutable snapshot of one verification channel (email or phone).
    /// A new instance is created on every state change.
    /// </summary>
    public sealed record VerificationChannel
    {
        public string? CodeHash { get; init; }
        public DateTime? SentAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public DateTime? VerifiedAt { get; init; }
        public int AttemptCount { get; init; }
        public int ResendCount { get; init; }

        public static readonly VerificationChannel Empty = new();

        public bool IsVerified => VerifiedAt.HasValue;
        public bool IsCodeActive(DateTime utcNow) =>
            ExpiresAt.HasValue && utcNow <= ExpiresAt.Value;
    }
}