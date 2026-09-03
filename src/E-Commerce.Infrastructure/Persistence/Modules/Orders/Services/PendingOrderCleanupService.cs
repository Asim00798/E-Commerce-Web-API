using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Services;

public sealed class PendingOrderCleanupService : IPendingOrderCleanupService
{
    private readonly IOrderRepository _orderRepository;

    public PendingOrderCleanupService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Guid>> ExpirePendingOrdersAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default)
    {
        var expirationTime = DateTime.UtcNow - expirationThreshold;
        var orderIds = await _orderRepository
            .GetPendingOrderIdsOlderThanAsync(expirationTime, cancellationToken);

        var expiredOrderIds = new List<Guid>();

        foreach (var orderId in orderIds)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order is null || order.Status != OrderStatus.PendingPayment)
                continue;

            try
            {
                order.MarkPaymentFailed();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                expiredOrderIds.Add(orderId);
            }
            catch (DomainException)
            {
                // Order changed state between query and now; ignore.
            }
        }

        return expiredOrderIds;
    }
}