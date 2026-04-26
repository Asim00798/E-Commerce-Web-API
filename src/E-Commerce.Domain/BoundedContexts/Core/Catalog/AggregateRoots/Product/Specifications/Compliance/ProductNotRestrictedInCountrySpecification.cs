using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Specifications.Compliance;

/// <summary>
/// Compliance rule ensuring that the product is allowed for sale in the customer's country.
/// The caller must pre‑compute the boolean flag "IsProductCountryAllowed".
/// </summary>
public class ProductNotRestrictedInCountrySpecification : IComplianceSpecification
{
    public string RuleCode => "PRODUCT_COUNTRY_RESTRICTION";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsProductCountryAllowed", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "This product is restricted for sale in the customer's country.",
        SeverityLevelEnum.High);
}