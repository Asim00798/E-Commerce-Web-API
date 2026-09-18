using E_Commerce.Application.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Execution
{
    public static class ExecutionExtensions 
    {
        public static IServiceCollection AddExecutionContext(this IServiceCollection services)
        {
            services.AddSingleton<IAppContext, ExecutionContext>();
            return services;
        }
    }
}
