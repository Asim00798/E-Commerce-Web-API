using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;
using E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories;

namespace E_Commerce.Application.BoundedContexts.Orders.DomainEventHandlers;

public sealed class OrderPaidDomainEventHandler
    : IDomainEventHandler<OrderPaidDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;
    private readonly IPersonRepository _personRepository;

    public OrderPaidDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext,
        IPersonRepository personRepository)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
        _personRepository = personRepository;
    }

    public async Task Handle(OrderPaidDomainEvent domainEvent, CancellationToken ct)
    {
        // Fetch the customer's Person record to get the delivery address.
        var person = await _personRepository.GetByIdentityUserIdAsync(
            domainEvent.CustomerId, ct);

        if (person is null)
            throw new InvalidOperationException(
                $"Person record not found for customer {domainEvent.CustomerId}.");

        var address = person.HomeAddress;
        if (address is null)
            throw new InvalidOperationException(
                $"Customer {domainEvent.CustomerId} does not have a home address.");

        var integrationEvent = new OrderReadyForShippingIntegrationEvent(
            orderId: domainEvent.OrderId,
            customerId: domainEvent.CustomerId,
            fullName: person.Name.ToString(),
            phoneNumber: person.PhoneNumber.Value,
            street: address.Street,
            city: address.City,
            locationMapUrl: address.LocationMapUrl!)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}