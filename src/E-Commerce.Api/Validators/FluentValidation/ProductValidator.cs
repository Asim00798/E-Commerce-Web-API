using FluentValidation;
using E_Commerce.Api.DTOs.v1.Catalog.Requests;

namespace E_Commerce.Api.Validators.FluentValidation;

public class ProductValidator : AbstractValidator<CreateProductRequest>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
