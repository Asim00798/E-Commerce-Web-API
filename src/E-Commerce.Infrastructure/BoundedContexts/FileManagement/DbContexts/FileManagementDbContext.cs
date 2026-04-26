namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.DbContexts;

/// <summary>
/// EF Core DbContext for the FileManagement bounded context.
/// Manages file-related technical entities (upload sessions, preview info, download logs, jobs).
/// </summary>
public sealed class FileManagementDbContext : DbContext
{
    public FileManagementDbContext(DbContextOptions<FileManagementDbContext> options) : base(options) { }

    // TODO: Add DbSet<T> file management entity sets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileManagementDbContext).Assembly);
    }
}
