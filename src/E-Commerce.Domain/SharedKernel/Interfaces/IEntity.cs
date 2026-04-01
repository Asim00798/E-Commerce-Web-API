using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.SharedKernel.Interfaces
{
    public interface IEntity<T> where T : BaseEntity
    { }
}
