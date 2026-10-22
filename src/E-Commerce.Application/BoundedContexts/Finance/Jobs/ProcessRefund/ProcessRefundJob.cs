using E_Commerce.Application.Shared.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ProcessRefund;

public sealed record ProcessRefundJob(Guid RefundId) : IJob;