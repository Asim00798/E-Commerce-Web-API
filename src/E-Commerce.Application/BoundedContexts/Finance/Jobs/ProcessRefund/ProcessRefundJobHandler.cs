using E_Commerce.Application.BoundedContexts.Finance.Commands.ProcessRefund;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ProcessRefund;

public sealed class ProcessRefundJobHandler : IJobHandler<ProcessRefundJob>
{
    private readonly ISender _sender;

    public ProcessRefundJobHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(ProcessRefundJob job, CancellationToken ct)
    {
        await _sender.Send(new ProcessRefundCommand(job.RefundId), ct);
    }
}