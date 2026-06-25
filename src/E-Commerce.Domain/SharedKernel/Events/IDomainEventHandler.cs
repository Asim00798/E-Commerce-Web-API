using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.SharedKernel.Events;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}