using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Features.Invoices.Projections;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;

namespace E_Commerce.ReadModel.Data;

public class ReadDbContext : DbContext, IReadDbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    public DbSet<InvoiceProjection> Invoices => Set<InvoiceProjection>();

    public IDbConnection Connection => Database.GetDbConnection();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    IQueryable<T> IReadDbContext.Query<T>() where T : class
    {
        return Set<T>().AsNoTracking();
    }
}
