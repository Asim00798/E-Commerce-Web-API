using E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.Validation;
using E_Commerce.Application.BoundedContexts.Catalog.Products.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Application.Extensions;

public static class ContextValidatorRegistrationExtensions
{
    public static IServiceCollection AddContextValidators(this IServiceCollection services)
    {
        services.AddScoped<BrandLogoFileValidator>();
        services.AddScoped<CategoryImageFileValidator>();
        services.AddScoped<ProductImageFileValidator>();

        return services;
    }
}