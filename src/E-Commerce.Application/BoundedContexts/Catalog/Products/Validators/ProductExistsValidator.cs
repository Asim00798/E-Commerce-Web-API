using FluentValidation;
using E_Commerce.Domain.Catalog.Repositories;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Validators;

public class ProductExistsValidator : AbstractValidator<Guid>
{
    public ProductExistsValidator(IProductRepository repository)
    {
        RuleFor(x => x).MustAsync(async (id, cancellationToken) =>
        {
            var product = await repository.GetByIdAsync(id, cancellationToken);
            return product != null;
        }).WithMessage("Product does not exist.");
    }
}
