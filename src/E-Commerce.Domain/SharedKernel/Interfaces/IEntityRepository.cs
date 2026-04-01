using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.SharedKernel.Interfaces
{
    public interface IEntityRepository<T> where T : BaseEntity,IEntity<T>
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        void Remove(T entity);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    }
}
