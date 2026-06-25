using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public PlaceOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IDomainEventDispatcher domainEventDispatcher)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Guid> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            var orderLines = command.Lines.Select(l => new OrderLine(l.ProductId, l.Quantity, l.Price)).ToList();
            var order = Order.Place(command.CustomerId, orderLines);

            await _orderRepository.AddAsync(order, cancellationToken);

            // SaveChangesAsync internally dispatches domain events and saves again
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return order.Id;
        }
    }
}
