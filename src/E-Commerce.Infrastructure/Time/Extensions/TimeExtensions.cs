using E_Commerce.Domain.SharedKernel.Services;
using E_Commerce.Infrastructure.Time.Services;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Time.Extensions
{
    public static class TimeExtensions
    {
        public static IServiceCollection AddTimeInfrastructure(
            this IServiceCollection services)
        {
            services.AddScoped<IClock, ClockService>();
            return services;
        }
    }
}
