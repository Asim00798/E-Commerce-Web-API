using E_Commerce.Infrastructure.Files.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Context
{
    public partial class AppDbContext
    {
        public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    }
}
