using E_Commerce.Domain.BoundedContexts.Core.Ordering.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands;

public record PlaceOrderCommand(Guid CustomerId, List<OrderLine> Lines) : IRequest<PlaceOrderResult>;
public record PlaceOrderResult(Guid OrderId, decimal TotalAmount);