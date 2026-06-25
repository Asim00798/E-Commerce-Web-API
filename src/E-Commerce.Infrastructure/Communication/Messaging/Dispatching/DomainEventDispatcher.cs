using Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Infrastructure.Communication.Messaging.Dispatching;

/// <summary>
/// Implements the domain event dispatcher, responsible for delivering
/// in‑memory domain events to all registered handlers within the
/// current transaction boundary.
/// </summary>
/// <remarks>
/// <para>
/// This class is a core part of the **synchronous event pipeline**.
/// It is called by the <see cref="UnitOfWork"/> during
/// <see cref="UnitOfWork.SaveChangesAsync"/> and dispatches each
/// domain event raised by an aggregate to every registered
/// <see cref="IDomainEventHandler{T}"/> implementation.
/// </para>
/// <para>
/// <b>Key characteristics:</b>
/// <list type="bullet">
///   <item>Runs inside the same database transaction – all handlers
///         share the same transactional scope.</item>
///   <item>Uses reflection to resolve the generic handler interface
///         at runtime because the concrete event type is only known
///         during execution.</item>
///   <item>Supports multiple handlers per event type (fan‑out).</item>
///   <item>Does <b>not</b> persist events; domain events are always
///         transient and discarded after dispatch.</item>
/// </list>
/// </para>
/// </remarks>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initialises the dispatcher with the root service provider.
    /// </summary>
    /// <param name="serviceProvider">
    /// The application's DI container, used to dynamically resolve
    /// <c>IDomainEventHandler&lt;T&gt;</c> implementations.
    /// </param>
    public DomainEventDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Build the closed generic handler type, e.g.
            // IDomainEventHandler<OrderPlacedDomainEvent>
            var handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEvent.GetType());

            // Retrieve all handler implementations registered in DI
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                // Invoke the handler's Handle method via reflection
                // and await the resulting Task
                await (Task)handlerType
                    .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!
                    .Invoke(handler, new object[] { domainEvent, cancellationToken })!;
            }
        }
    }
}