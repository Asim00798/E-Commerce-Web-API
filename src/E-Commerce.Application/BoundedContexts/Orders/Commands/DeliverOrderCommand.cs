using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands;

public record DeliverOrderCommand(Guid OrderId) : IRequest;