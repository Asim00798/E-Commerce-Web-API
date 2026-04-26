using E_Commerce.Domain.BoundedContexts.Core.Compliance.Evaluation;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Specifications;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Compliance.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Specifications.Compliance;

/// <summary>
/// Compliance rule ensuring that the product has at least one valid safety certificate.
/// The caller must pre‑compute the boolean flag "IsProductSafetyCertificateValid".
/// </summary>
public class ProductMustHaveValidSafetyCertificateSpecification : IComplianceSpecification
{
    public string RuleCode => "PRODUCT_SAFETY_CERTIFICATE_VALID";

    public bool IsSatisfiedBy(EvaluationContext context)
    {
        return context.Facts.TryGetValue("IsProductSafetyCertificateValid", out var value) && value is true;
    }

    public ViolationDetail GetViolationDetail() => new ViolationDetail(
        RuleCode,
        "The product does not have a valid safety certificate.",
        SeverityLevelEnum.High);
}