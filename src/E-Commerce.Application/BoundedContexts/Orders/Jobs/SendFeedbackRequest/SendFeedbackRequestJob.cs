using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs.SendFeedbackRequest;

public sealed record SendFeedbackRequestJob(Guid OrderId) : IJob;