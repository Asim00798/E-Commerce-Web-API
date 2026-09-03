namespace E_Commerce.Application.Shared.Communication.PostCommit;

/// <summary>
/// Enqueues a callback that will be executed after the current UnitOfWork transaction commits.
/// The callback receives an IServiceProvider and a CancellationToken.
/// </summary>
public interface IPostCommitProcessor
{
    void Enqueue(Func<IServiceProvider, CancellationToken, Task> callback);
    Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}