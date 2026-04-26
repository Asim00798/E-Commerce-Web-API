namespace E_Commerce.ReadModel.Abstractions;

/// <summary>
/// Optional generic repository abstraction for read models.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public interface IReadModelRepository<TReadModel, TKey>
    where TReadModel : class
{
    Task<TReadModel?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
}
