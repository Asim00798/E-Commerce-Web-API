using FluentValidation;
using E_Commerce.Api.DTOs.v1.Catalog.Requests;

namespace E_Commerce.Api.Validators.FluentValidation;

public class CategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
