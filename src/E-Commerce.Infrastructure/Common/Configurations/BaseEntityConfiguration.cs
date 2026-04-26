using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Common.Configurations;

/// <summary>
/// Base EF Core configuration shared by all aggregate root entities.
/// Applies common conventions (Id as primary key, audit timestamps, etc.).
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // TODO: Apply common conventions (primary key, concurrency token, soft-delete filter)
    }
}
