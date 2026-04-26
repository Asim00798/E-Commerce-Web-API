using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Specifications.Compliance;

/// <summary>
/// Compliance rule ensuring that the customer meets the product's minimum age requirement.
/// The caller must pre‑compute the boolean flag "IsProductAgeRequirementMet".
/// </summary>
public class ProductMeetsAgeRatingSpecification : IComplianceSpecification
{
    public string RuleCode => "PRODUCT_AGE_RATING";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsProductAgeRequirementMet", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "Customer does not meet the minimum age requirement for this product.",
        SeverityLevelEnum.High);
}