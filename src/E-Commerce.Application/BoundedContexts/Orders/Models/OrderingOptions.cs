namespace E_Commerce.Application.BoundedContexts.Orders.Models;

public sealed class OrderingOptions
{
    public int MaximumItemsPerCart { get; set; } = 500;
    public int PendingOrderExpirationHours { get; set; } = 24;
}