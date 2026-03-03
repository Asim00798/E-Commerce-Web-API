using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.Abstractions;

public interface IReadDbContext
{
    DbSet<T> Set<T>() where T : class;
    IQueryable<T> Query<T>() where T : class;
}
