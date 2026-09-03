using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories;

namespace E_Commerce.Application.BoundedContexts.Orders.DomainEventHandlers;

public sealed class OrderPlacedDomainEventHandler
    : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;
    private readonly IPersonRepository _personRepository;

    public OrderPlacedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext,
        IPersonRepository personRepository)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
        _personRepository = personRepository;
    }

    public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken ct)
    {
        // CustomerId is the IdentityUserId
        var person = await _personRepository.GetByIdentityUserIdAsync(
            domainEvent.CustomerId, ct);

        if (person is null)
            throw new InvalidOperationException(
                $"Person record not found for customer {domainEvent.CustomerId}.");

        var integrationEvent = new OrderPlacedIntegrationEvent(
            orderId: domainEvent.OrderId,
            customerId: domainEvent.CustomerId,
            totalAmount: domainEvent.TotalAmount.Amount,
            currency: domainEvent.TotalAmount.Currency,
            customerEmail: person.Email.Value,
            customerName: person.Name.ToString())
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}