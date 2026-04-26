using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Specifications.Compliance;

/// <summary>
/// Compliance rule ensuring that the customer meets the brand's minimum age requirement.
/// The caller must pre‑compute the boolean flag "IsAgeRequirementMet" based on the
/// brand's required minimum age and the customer's age.
/// </summary>
public class BrandMeetsAgeRatingSpecification : IComplianceSpecification
{
    public string RuleCode => "BRAND_AGE_RATING";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsAgeRequirementMet", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "Customer does not meet the minimum age requirement for this brand.",
        SeverityLevelEnum.High);
}