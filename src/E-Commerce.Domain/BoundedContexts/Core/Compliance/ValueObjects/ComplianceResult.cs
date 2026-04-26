namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects
{
    public sealed record ComplianceResult
    {
        public bool IsCompliant { get; init; }
        public ViolationDetail? Violation { get; init; }// null if compliant

        private ComplianceResult(bool isCompliant, ViolationDetail? violation)
        {
            IsCompliant = isCompliant;
            Violation = violation;
        }

        public static ComplianceResult Success()
        {
            return new ComplianceResult(true, null);
        }

        public static ComplianceResult Failure(ViolationDetail violation)
        {
            if (violation == null) throw new ArgumentNullException(nameof(violation));
            return new ComplianceResult(false, violation);
        }
    }
}
