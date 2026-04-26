namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Repositories;

/// <summary>
/// EF Core implementation of the FileDownloadLog repository.
/// </summary>
public sealed class FileDownloadLogRepository
{
    private readonly DbContexts.FileManagementDbContext _context;

    public FileDownloadLogRepository(DbContexts.FileManagementDbContext context)
    {
        _context = context;
    }

    // TODO: Implement IFileDownloadLogRepository members
}
