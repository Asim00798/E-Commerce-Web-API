using E_Commerce.Application.Shared.Communication.PostCommit;

namespace E_Commerce.Infrastructure.Communication.PostCommit.Processing;

/// <summary>
/// Scoped implementation of <see cref="IPostCommitProcessor"/>.
/// Stores callbacks that will be executed after the current UnitOfWork transaction commits.
/// </summary>
internal sealed class PostCommitProcessor : IPostCommitProcessor
{
    private readonly List<Func<IServiceProvider, Task>> _callbacks = new();
    private readonly ILogger<PostCommitProcessor> _logger;

    public PostCommitProcessor(ILogger<PostCommitProcessor> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<IServiceProvider, Task> callback)
    {
        _callbacks.Add(callback);
    }

    public async Task InvokeAsync(IServiceProvider serviceProvider)
    {
        foreach (var callback in _callbacks)
        {
            try
            {
                await callback(serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post‑commit application event callback failed.");
            }
        }
    }
}