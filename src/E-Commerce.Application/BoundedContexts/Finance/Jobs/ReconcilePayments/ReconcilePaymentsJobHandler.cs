using E_Commerce.Application.BoundedContexts.Finance.Commands.ReconcilePayments;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Finance.Jobs.ReconcilePayments;

public sealed class ReconcilePaymentsJobHandler : IJobHandler<ReconcilePaymentsJob>
{
    private readonly ISender _sender;

    public ReconcilePaymentsJobHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(ReconcilePaymentsJob job, CancellationToken ct)
    {
        await _sender.Send(new ReconcilePaymentsCommand(), ct);
    }
}