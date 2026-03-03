namespace E_Commerce.Domain.SharedKernel.Exceptions
{
    /// <summary>
    /// Thrown when a domain business rule is violated.
    /// Example: placing an order with total = 0.
    /// </summary>
    public sealed class BusinessRuleViolationException : DomainException
    {
        public string Rule { get; }

        public BusinessRuleViolationException(string rule)
            : base($"Business rule violated: {rule}")
        {
            Rule = rule;
        }
        public BusinessRuleViolationException(string rule, string details)
            : base($"Business rule violated: {rule}. {details}")
        {
            Rule = rule;
        }
    }
}
