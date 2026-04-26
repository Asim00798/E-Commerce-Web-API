using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.AggregateRoots.ComplianceCase.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Compliance.Services;

/// <summary>
/// Pure, synchronous domain service that evaluates compliance rules.
/// All external facts (document validity, country restrictions, age checks)
/// must be pre‑computed by the caller and passed as boolean flags in <see cref="EvaluationContext"/>.
/// </summary>
public sealed class ComplianceEvaluationService
{
    private readonly IEnumerable<IComplianceSpecification> _specifications;

    public ComplianceEvaluationService(IEnumerable<IComplianceSpecification> specifications)
    {
        _specifications = specifications ?? throw new ArgumentNullException(nameof(specifications));
    }

    public ComplianceCase EvaluateTarget(EvaluationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Create aggregate
        var targetInfo = new TargetEntityInfo(context.TargetEntityId, context.TargetEntityType);
        var complianceCase = ComplianceCase.Create(targetInfo, context.SourceContext);

        // Evaluate all specifications synchronously (pure domain logic)
        var results = new List<ComplianceResult>();
        foreach (var spec in _specifications)
        {
            if (spec.IsSatisfiedBy(context))
                results.Add(ComplianceResult.Success());
            else
                results.Add(ComplianceResult.Failure(spec.GetViolationDetail()));
        }

        // Record outcome
        complianceCase.Evaluate(results, EvaluationTimestamp.Now());
        return complianceCase;
    }
}