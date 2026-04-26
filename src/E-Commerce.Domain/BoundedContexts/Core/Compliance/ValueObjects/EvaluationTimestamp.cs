namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects
{
    public sealed record EvaluationTimestamp
    {
        public DateTime Timestamp { get; init; }

        public EvaluationTimestamp(DateTime timestamp)
        {
            if (timestamp > DateTime.UtcNow)
                throw new Exceptions.ComplianceDomainException("Evaluation timestamp cannot be in the future.");

            Timestamp = timestamp;
        }

        public static EvaluationTimestamp Now() => new EvaluationTimestamp(DateTime.UtcNow);
    }
}
