using E_Commerce.Application.Shared.Communication.PostCommit;
using E_Commerce.Infrastructure.Communication.PostCommit.Processing;

namespace E_Commerce.Infrastructure.Communication.PostCommit.Extensions;

public static class PostCommitExtensions
{
    public static IServiceCollection AddPostCommitProcessor(this IServiceCollection services)
    {
        services.AddScoped<IPostCommitProcessor, PostCommitProcessor>();
        return services;
    }
}