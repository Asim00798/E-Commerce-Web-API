using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands;

public class DeliverOrderCommandHandler : IRequestHandler<DeliverOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;

    public DeliverOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task Handle(DeliverOrderCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException("Order not found.", command.OrderId);

        // Business operation – raises OrderDeliveredDomainEvent
        order.MarkAsDelivered();

        // SaveChangesAsync dispatches domain events (including the one that writes to Outbox)
        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
    }
}