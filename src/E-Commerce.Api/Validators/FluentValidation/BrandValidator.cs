using FluentValidation;
using E_Commerce.Api.DTOs.v1.Catalog.Requests;

namespace E_Commerce.Api.Validators.FluentValidation;

public class BrandValidator : AbstractValidator<CreateBrandRequest>
{
    public BrandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
