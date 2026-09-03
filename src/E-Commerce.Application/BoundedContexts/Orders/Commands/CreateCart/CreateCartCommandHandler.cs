using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.CreateCart;

public sealed class CreateCartCommandHandler
    : IRequestHandler<CreateCartCommand, Result<Guid>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCartCommandHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateCartCommand request,
        CancellationToken ct)
    {
        // Ensure user doesn't already have an active cart
        var existing = await _cartRepository.GetByCustomerIdAsync(_currentUser.UserId!.Value, ct);
        if (existing is not null)
            return Result<Guid>.Failure("Cart already exists.");

        var cart = Cart.Create(_currentUser.UserId.Value);
        await _cartRepository.AddAsync(cart, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(cart.Id);
    }
}