using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs;

/// <summary>
/// Background job that expires orders that have been in "Pending" state beyond the allowed time.
/// </summary>
public class ExpirePendingOrdersJob : IJob
{}