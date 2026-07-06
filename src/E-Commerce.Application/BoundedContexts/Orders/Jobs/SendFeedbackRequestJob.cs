using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs;

/// <summary>
/// Scheduled job that sends a feedback request email 7 days after delivery.
/// </summary>
public class SendFeedbackRequestJob : IJob
{
    public Guid OrderId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
}