using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;

public class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).NotEmpty();
        RuleFor(v => v.Price).GreaterThanOrEqualTo(0);
        RuleFor(v => v.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
