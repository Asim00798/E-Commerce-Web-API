using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.BoundedContexts.FileManagement.Configurations;

/// <summary>
/// EF Core fluent configuration for the FileDownloadLog entity.
/// </summary>
public sealed class FileDownloadLogConfiguration : IEntityTypeConfiguration<object>
{
    public void Configure(EntityTypeBuilder<object> builder)
    {
        // TODO: Configure FileDownloadLog table, keys, and columns
    }
}
