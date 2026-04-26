using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Specifications.Compliance;

/// <summary>
/// Compliance rule ensuring that the brand is allowed to sell in the customer's country.
/// The caller must pre‑compute the boolean flag "IsCountryAllowed" based on brand's
/// restricted country list and the order's shipping country.
/// </summary>
public class BrandMustNotBeRestrictedInCountrySpecification : IComplianceSpecification
{
    public string RuleCode => "BRAND_COUNTRY_RESTRICTION";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsCountryAllowed", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "This brand is restricted for sale in the customer's country.",
        SeverityLevelEnum.High);
}