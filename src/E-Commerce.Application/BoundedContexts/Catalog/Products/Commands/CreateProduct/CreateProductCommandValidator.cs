using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(200)
            .NotEmpty();

        RuleFor(v => v.Price)
            .GreaterThan(0);

        RuleFor(v => v.CategoryId)
            .NotEmpty();
    }
}
