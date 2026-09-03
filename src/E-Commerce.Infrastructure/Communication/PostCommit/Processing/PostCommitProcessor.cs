using E_Commerce.Application.Shared.Communication.PostCommit;

namespace E_Commerce.Infrastructure.Communication.PostCommit.Processing;

internal sealed class PostCommitProcessor : IPostCommitProcessor
{
    private readonly Queue<Func<IServiceProvider, CancellationToken, Task>> _callbacks = new();
    private readonly ILogger<PostCommitProcessor> _logger;

    public PostCommitProcessor(ILogger<PostCommitProcessor> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> callback)
    {
        _callbacks.Enqueue(callback);
    }

    public async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        if (_callbacks.Count == 0) return;

        _logger.LogDebug("Invoking {Count} post‑commit callback(s).", _callbacks.Count);

        while (_callbacks.TryDequeue(out var callback))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Post‑commit callback execution cancelled.");
                break;
            }

            try
            {
                _logger.LogDebug("Executing post‑commit callback ({Remaining} remaining).",
                    _callbacks.Count + 1); // +1 because we already dequeued it
                await callback(serviceProvider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post‑commit callback failed.");
            }
        }
    }
}