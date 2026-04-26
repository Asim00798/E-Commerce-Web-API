namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects
{
    public sealed record ViolationDetail
    {
        public string RuleCode { get; init; }
        public string Description { get; init; }
        public Enums.SeverityLevelEnum SeverityLevel { get; init; }

        public ViolationDetail(string ruleCode, string description, Enums.SeverityLevelEnum severityLevel)
        {
            if (string.IsNullOrWhiteSpace(ruleCode))
                throw new Exceptions.ComplianceDomainException("Rule code cannot be null or empty.");

            RuleCode = ruleCode;
            Description = description;
            SeverityLevel = severityLevel;
        }
    }
}
