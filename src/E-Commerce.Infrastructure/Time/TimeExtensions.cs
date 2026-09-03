using E_Commerce.Domain.SharedKernel.Services;

namespace E_Commerce.Infrastructure.Time
{
    public static class TimeExtensions
    {
        public static IServiceCollection AddTimeInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IClock, ClockService>();
            return services;
        }
    }
}
