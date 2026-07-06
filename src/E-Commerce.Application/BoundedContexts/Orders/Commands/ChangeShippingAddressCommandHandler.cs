using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.Orders.Commands;

public class ChangeShippingAddressCommandHandler
    : IRequestHandler<ChangeShippingAddressCommand, ChangeShippingAddressResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;

    public ChangeShippingAddressCommandHandler(
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<ChangeShippingAddressResult> Handle(
        ChangeShippingAddressCommand command,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            throw new NotFoundException("Order not found.", command.OrderId);

        order.ChangeShippingAddress(command.NewAddress);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ChangeShippingAddressResult.Success();
    }
}