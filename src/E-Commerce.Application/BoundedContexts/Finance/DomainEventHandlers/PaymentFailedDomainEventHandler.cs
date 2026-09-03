using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Application.BoundedContexts.Finance.DomainEventHandlers;

public sealed class PaymentFailedDomainEventHandler
    : IDomainEventHandler<PaymentFailedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public PaymentFailedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(PaymentFailedDomainEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new PaymentFailedIntegrationEvent(
            domainEvent.PaymentId,
            domainEvent.OrderId)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}