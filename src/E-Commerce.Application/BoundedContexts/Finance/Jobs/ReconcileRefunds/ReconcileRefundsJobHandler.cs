using E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcileRefunds;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ReconcileRefunds;

public sealed class ReconcileRefundsJobHandler : IJobHandler<ReconcileRefundsJob>
{
    private readonly ISender _sender;

    public ReconcileRefundsJobHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(ReconcileRefundsJob job, CancellationToken ct)
    {
        await _sender.Send(new ReconcileRefundsCommand(), ct);
    }
}