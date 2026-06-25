using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.SharedKernel.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}