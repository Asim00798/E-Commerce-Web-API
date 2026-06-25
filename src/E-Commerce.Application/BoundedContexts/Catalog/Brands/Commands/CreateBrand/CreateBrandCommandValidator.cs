using FluentValidation;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL cannot exceed 500 characters.")
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("Logo URL must be a valid absolute URL.");
    }
}