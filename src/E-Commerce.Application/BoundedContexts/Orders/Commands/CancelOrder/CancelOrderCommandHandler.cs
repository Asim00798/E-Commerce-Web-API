using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IPermissionService permissionService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _permissionService = permissionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelOrderCommand request,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure("Order not found.");

        bool isOwner = _currentUser.UserId == order.CustomerId;
        bool canManage = await _permissionService.HasPermissionAsync(
            _currentUser.UserId!.Value,
            OrderingPermissions.Manage);

        if (!isOwner && !canManage)
            return Result.Failure("You are not authorized to cancel this order.");

        try
        {
            order.Cancel();
        }
        catch (OrderException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _orderRepository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}