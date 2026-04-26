using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQueryValidator : AbstractValidator<ListBrandsQuery>
{
    public ListBrandsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
