namespace E_Commerce.Infrastructure.Jobs;

/// <summary>
/// Background job that marks expired documents as invalid on a schedule.
/// </summary>
public sealed class ExpireDocumentsJob
{
    // TODO: Inject required services and implement IJob / IRecurringJob
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Query for documents past their expiry date and update their status
        throw new NotImplementedException();
    }
}
