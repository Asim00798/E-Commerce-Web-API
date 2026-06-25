using Microsoft.EntityFrameworkCore.Design;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    /// <summary>
    /// Factory for design-time DbContext creation (needed for EF Core tools like migrations)
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            // Configure your connection string here
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
