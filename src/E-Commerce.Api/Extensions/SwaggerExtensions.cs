using E_Commerce.Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace E_Commerce.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var swagger = configuration
            .GetSection(SwaggerConfiguration.SectionName)
            .Get<SwaggerConfiguration>()
            ?? throw new InvalidOperationException("Swagger configuration is missing.");

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(swagger.Version, new OpenApiInfo
            {
                Version = swagger.Version,
                Title = swagger.Title,
                Description = swagger.Description,
                Contact = new OpenApiContact
                {
                    Name = swagger.ContactName,
                    Email = swagger.ContactEmail
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT access token. Example: Bearer {your token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseDevelopmentSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        var swagger = app.Configuration
            .GetSection(SwaggerConfiguration.SectionName)
            .Get<SwaggerConfiguration>()
            ?? throw new InvalidOperationException("Swagger configuration is missing.");

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                $"/swagger/{swagger.Version}/swagger.json",
                $"{swagger.Title} {swagger.Version}");
        });

        return app;
    }
}