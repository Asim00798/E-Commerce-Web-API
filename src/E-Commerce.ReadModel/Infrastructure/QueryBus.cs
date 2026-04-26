using E_Commerce.ReadModel.Abstractions;

namespace E_Commerce.ReadModel.Infrastructure;

/// <summary>
/// Mediator-like query bus that resolves and dispatches queries to their registered handlers.
/// </summary>
public sealed class QueryBus
{
    private readonly IServiceProvider _serviceProvider;

    public QueryBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches the given query to its registered handler.
    /// </summary>
    public Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        // TODO: Resolve IQueryHandler<TQuery, TResult> from DI and invoke HandleAsync
        throw new NotImplementedException();
    }
}
