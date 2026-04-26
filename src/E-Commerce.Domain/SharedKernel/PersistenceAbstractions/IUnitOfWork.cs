namespace E_Commerce.Domain.SharedKernel.PersistenceAbstractions
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

