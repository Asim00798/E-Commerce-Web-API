using MediatR;

namespace E_Commerce.Application.Orders.Commands;

public record ChangeShippingAddressCommand(
    Guid OrderId,
    string NewAddress
) : IRequest<ChangeShippingAddressResult>;