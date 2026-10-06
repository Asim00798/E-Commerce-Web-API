using E_Commerce.Application.Shared.Orders;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Services;

/// <summary>
/// Implements the shared IOrderPricingReader contract for Finance.
/// Returns authoritative pricing derived from the Order aggregate —
/// used to prevent client-tampered payment fields.
/// </summary>
public sealed class OrderPricingReader : IOrderPricingReader
{
    private readonly AppDbContext _dbContext;

    public OrderPricingReader(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderPricingSnapshot?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken ct = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                x.Total.Amount,
                x.Total.Currency
            })
            .FirstOrDefaultAsync(ct);

        if (order is null)
            return null;

        return new OrderPricingSnapshot(
            order.Id,
            order.CustomerId,
            order.Amount,
            order.Currency);
    }
}