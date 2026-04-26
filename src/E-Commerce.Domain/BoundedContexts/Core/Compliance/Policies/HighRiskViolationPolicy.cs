using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Policies
{
    public sealed class HighRiskViolationPolicy
    {
        private readonly Enums.SeverityLevelEnum _threshold;

        public HighRiskViolationPolicy(Enums.SeverityLevelEnum threshold = Enums.SeverityLevelEnum.High)
        {
            _threshold = threshold;
        }

        /// <summary>
        /// Analyzes a compliance case to determine if it requires manual review
        /// due to critical/high-severity violations. This is a policy, so it
        /// does NOT mutate state, only returns a boolean judgment.
        /// </summary>
        public bool RequiresManualReview(ComplianceCase complianceCase)
        {
            if (complianceCase == null) throw new ArgumentNullException(nameof(complianceCase));

            if (complianceCase.Status != Enums.ComplianceStatusEnum.NonCompliant)
            {
                return false;
            }

            return complianceCase.Violations.Any(v => v.Violation.SeverityLevel >= _threshold);
        }
    }
}
