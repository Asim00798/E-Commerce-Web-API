using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).NotEmpty();
        RuleFor(v => v.Price).GreaterThan(0);
        RuleFor(v => v.CategoryId).NotEmpty();
    }
}
