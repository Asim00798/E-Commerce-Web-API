namespace E_Commerce.Api.Extensions;

public static class HttpsExtensions
{
    public static IServiceCollection AddHttpsConfiguration(
        this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        return services;
    }

    public static IApplicationBuilder UseHttpsConfiguration(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        return app;
    }
}