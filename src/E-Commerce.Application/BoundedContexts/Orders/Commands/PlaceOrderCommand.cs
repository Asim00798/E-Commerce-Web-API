
namespace E_Commerce.Application.BoundedContexts.Orders.Commands
{
    public record PlaceOrderCommand(Guid CustomerId, List<OrderLineDto> Lines);
    public record OrderLineDto(Guid ProductId, int Quantity, decimal Price);
}
