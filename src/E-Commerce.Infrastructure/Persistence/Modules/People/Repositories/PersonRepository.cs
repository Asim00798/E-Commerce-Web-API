using E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.Behaviors;
using E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.People.Repositories;

public sealed class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext dbContext) : base(dbContext)
    {}

    public async Task<Person?> GetByIdentityUserIdAsync(
        Guid identityUserId,
        CancellationToken ct = default)
    {
        return await _dbContext.Set<Person>()
            .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId, ct);
    }
}