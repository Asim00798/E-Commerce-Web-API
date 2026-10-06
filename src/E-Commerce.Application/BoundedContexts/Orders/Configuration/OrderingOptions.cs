namespace E_Commerce.Application.BoundedContexts.Orders.Configuration;

public sealed class OrderingOptions
{
    public int MaximumItemsPerCart { get; set; }
    public int PendingOrderExpirationHours { get; set; }
}