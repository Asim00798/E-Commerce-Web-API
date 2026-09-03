using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories
{
    public interface IPersonRepository : IRepository<Person>
    {
        Task<Person?> GetByIdentityUserIdAsync(
            Guid identityUserId,
            CancellationToken ct = default);
    }
}
