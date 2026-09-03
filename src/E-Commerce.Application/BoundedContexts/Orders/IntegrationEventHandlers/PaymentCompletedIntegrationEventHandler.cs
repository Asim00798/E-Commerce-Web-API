using E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

public sealed class PaymentCompletedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentCompletedIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentCompletedIntegrationEventHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        PaymentCompletedIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(integrationEvent.OrderId, ct);
        if (order is null)
            return;

        // This will raise OrderPaidDomainEvent internally.
        order.MarkPaid();

        await _orderRepository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        // The UnitOfWork will dispatch the domain event, which writes to the Outbox.
    }
}