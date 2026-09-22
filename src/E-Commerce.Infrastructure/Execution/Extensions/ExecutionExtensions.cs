using E_Commerce.Application.Shared.Abstractions;
using AppExecutionContext = E_Commerce.Infrastructure.Execution.AppContexts.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Execution.Extensions
{
    public static class ExecutionExtensions 
    {
        public static IServiceCollection AddExecutionContext(this IServiceCollection services)
        {
            services.AddSingleton<IAppContext, AppExecutionContext>();
            return services;
        }
    }
}
