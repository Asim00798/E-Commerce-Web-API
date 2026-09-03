using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler
    : IRequestHandler<AddCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderingOptions _orderingOptions;

    public AddCartItemCommandHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IOptions<OrderingOptions> orderingOptions)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _orderingOptions = orderingOptions.Value;
    }

    public async Task<Result> Handle(
        AddCartItemCommand request,
        CancellationToken ct)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(_currentUser.UserId!.Value, ct);
        if (cart is null)
            return Result.Failure("Cart not found. Create a cart first.");

        //Enforce maximum distinct items(application - level policy)
        bool itemExists = cart.Items.Any(x => x.ProductVariantId == request.ProductVariantId);
        if (!itemExists && cart.Items.Count >= _orderingOptions.MaximumItemsPerCart)
        {
            return Result.Failure($"Cannot add more than {_orderingOptions.MaximumItemsPerCart} different items to the cart.");
        }

        try
        {
            cart.AddItem(
                request.ProductId,
                request.ProductVariantId,
                request.Sku,
                request.ProductName,
                request.VariantName,
                request.UnitPrice,
                request.Quantity);
        }
        catch (CartException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _cartRepository.UpdateAsync(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}