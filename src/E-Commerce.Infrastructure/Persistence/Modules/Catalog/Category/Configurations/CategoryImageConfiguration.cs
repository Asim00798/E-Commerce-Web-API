using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Entities;
using E_Commerce.Infrastructure.Persistence.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Infrastructure.Persistence.Modules.Catalog.Category.Configurations;

public sealed class CategoryImageConfiguration : BaseEntityConfiguration<CategoryImage>
{
    public override void Configure(EntityTypeBuilder<CategoryImage> builder)
    {
        base.Configure(builder);

        builder.ToTable("CategoryImages", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.FileId)
            .IsRequired();

        builder.Property(x => x.AltText)
            .HasMaxLength(250);

        builder.HasIndex(x => new { x.CategoryId, x.FileId })
            .IsUnique();
    }
}