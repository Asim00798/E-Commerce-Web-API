using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Application.BoundedContexts.Finance.DomainEventHandlers;

public sealed class RefundFailedDomainEventHandler
    : IDomainEventHandler<RefundFailedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public RefundFailedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(RefundFailedDomainEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new RefundFailedIntegrationEvent(
            domainEvent.RefundId,
            domainEvent.PaymentId,
            domainEvent.Amount.Amount,
            domainEvent.Amount.Currency,
            domainEvent.Reason)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}