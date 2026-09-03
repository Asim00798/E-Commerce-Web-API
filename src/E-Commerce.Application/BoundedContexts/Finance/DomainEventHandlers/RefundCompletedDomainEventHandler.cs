using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;
using E_Commerce.Domain.BoundedContexts.Core.Finance.Repositories;

namespace E_Commerce.Application.BoundedContexts.Finance.DomainEventHandlers;

public sealed class RefundCompletedDomainEventHandler
    : IDomainEventHandler<RefundCompletedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;
    private readonly IPaymentRepository _paymentRepository;
    public RefundCompletedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext,
        IPaymentRepository paymentRepository)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
        _paymentRepository = paymentRepository;
    }

    public async Task Handle(RefundCompletedDomainEvent domainEvent, CancellationToken ct)
    {
        var payment = await _paymentRepository.GetByIdAsync(domainEvent.PaymentId, ct);
        if (payment is null)
            throw new InvalidOperationException($"Payment with ID {domainEvent.PaymentId} not found.");

        var integrationEvent = new RefundCompletedIntegrationEvent(
            domainEvent.RefundId,
            domainEvent.PaymentId,
            payment.OrderId,
            domainEvent.Amount.Amount,
            domainEvent.Amount.Currency)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}