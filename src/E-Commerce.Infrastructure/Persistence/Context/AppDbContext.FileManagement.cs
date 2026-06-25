using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Behaviors;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.FileFolder.Entities;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        // FileManagement
        public DbSet<FileType> Files { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<FileMetadata> FileMetadata { get; set; }
        public DbSet<FileTag> FileTags { get; set; }

        public DbSet<FileFolder> FileFolders { get; set; }
        public DbSet<FolderAccessRule> FolderAccessRules { get; set; }
    }
}
