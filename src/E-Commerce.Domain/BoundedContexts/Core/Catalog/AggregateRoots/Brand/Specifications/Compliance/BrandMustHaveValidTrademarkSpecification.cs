using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Specifications.Compliance;

public class BrandMustHaveValidTrademarkSpecification : IComplianceSpecification
{
    public string RuleCode => "BRAND_TRADEMARK_VALID";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsTrademarkValid", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "The brand does not have a valid trademark certificate.",
        SeverityLevelEnum.High);
}