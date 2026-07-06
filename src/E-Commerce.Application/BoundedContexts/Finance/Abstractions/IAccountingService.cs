
namespace E_Commerce.Application.BoundedContexts.Finance.Abstractions
{
    public interface IAccountingService
    {
        public Task CreateAdjustmentEntriesAsync(
            Guid reconciliationId,
            int discrepancyCount,
            CancellationToken cancellationToken);
    }
}
