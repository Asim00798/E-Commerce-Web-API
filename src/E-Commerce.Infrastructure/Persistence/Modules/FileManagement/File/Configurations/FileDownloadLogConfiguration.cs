using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_Commerce.Infrastructure.BoundedContexts.FileManagement.Entities;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Configurations;

/// <summary>
/// EF Core fluent configuration for the FileDownloadLog entity.
/// </summary>
public sealed class FileDownloadLogConfiguration : IEntityTypeConfiguration<FileDownloadLog>
{
    public void Configure(EntityTypeBuilder<FileDownloadLog> builder)
    {
        // TODO: Configure FileDownloadLog table, keys, and indexes
    }
}
