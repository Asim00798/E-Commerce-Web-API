using E_Commerce.Domain.SharedKernel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Time
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
