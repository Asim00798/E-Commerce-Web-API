using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Application.BoundedContexts.Finance.DomainEventHandlers;

public sealed class RefundCompletedDomainEventHandler
    : IDomainEventHandler<RefundCompletedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public RefundCompletedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(RefundCompletedDomainEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new RefundCompletedIntegrationEvent(
            domainEvent.RefundId,
            domainEvent.PaymentId,
            domainEvent.OrderId,
            domainEvent.Amount.Amount,
            domainEvent.Amount.Currency)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}