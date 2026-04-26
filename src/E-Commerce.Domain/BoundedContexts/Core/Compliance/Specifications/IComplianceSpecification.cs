using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.SharedKernel.Specifications;


namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications
{
    /// <summary>
    /// Represents a pure, stateless predicate rule evaluated against an execution context.
    /// External contexts (e.g. Catalog) implement this interface.
    /// </summary>
    public interface IComplianceSpecification
    {
        /// <summary>
        /// A unique code representing the specific rule being executed.
        /// </summary>
        string RuleCode { get; }

        /// <summary>
        /// The stateless evaluation predicate. Returns true if the rule is met.
        /// </summary>
        bool IsSatisfiedBy(EvaluationContext context);

        /// <summary>
        /// Generates the violation detail if IsSatisfiedBy returns false.
        /// </summary>
        ViolationDetail GetViolationDetail();
    }
}
