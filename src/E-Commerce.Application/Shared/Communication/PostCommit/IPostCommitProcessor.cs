namespace E_Commerce.Application.Shared.Communication.PostCommit;

/// <summary>
/// Enqueues a callback that will be executed after the current UnitOfWork transaction commits.
/// The callback receives an IServiceProvider to safely resolve scoped services at execution time.
/// </summary>
public interface IPostCommitProcessor
{
    void Enqueue(Func<IServiceProvider, Task> callback);
    Task InvokeAsync(IServiceProvider serviceProvider);
}
