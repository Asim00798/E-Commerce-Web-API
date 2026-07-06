using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;  // IOrderRepository
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;        // IUnitOfWork
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;

    public PlaceOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<PlaceOrderResult> Handle(PlaceOrderCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var order = Order.Place(command.CustomerId, command.Lines);

        await _orderRepository.AddAsync(order, ct);

        // SaveChangesAsync dispatches domain events (both internal and integration‑event creators)
        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);

        return new PlaceOrderResult(order.Id, order.TotalAmount);
    }
}